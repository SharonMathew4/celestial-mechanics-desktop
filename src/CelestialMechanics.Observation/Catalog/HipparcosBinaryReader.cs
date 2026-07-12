using System.IO;
using System.Text;

namespace CelestialMechanics.Observation.Catalog;

/// <summary>
/// Handles binary reading and writing of standardized star catalog databases.
/// Compact representation with fixed-size records for maximum read/seek performance.
/// </summary>
public static class HipparcosBinaryReader
{
    private static readonly byte[] MagicBytes = "HIPB"u8.ToArray();
    private const ushort CurrentVersion = 1;
    private const int RecordSize = 48;

    /// <summary>
    /// Reads star entries from a custom Hipparcos binary database.
    /// </summary>
    public static List<StarEntry> ReadCatalog(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new BinaryReader(stream, Encoding.ASCII);

        byte[] magic = reader.ReadBytes(4);
        if (magic.Length < 4 || !magic.SequenceEqual(MagicBytes))
            throw new InvalidDataException("Invalid magic bytes in Hipparcos binary catalog.");

        ushort version = reader.ReadUInt16();
        if (version != CurrentVersion)
            throw new InvalidDataException($"Unsupported catalog version: {version}. Expected version {CurrentVersion}.");

        int count = reader.ReadInt32();
        var entries = new List<StarEntry>(count);

        byte[] spectralBuffer = new byte[12];

        for (int i = 0; i < count; i++)
        {
            int id = reader.ReadInt32();
            double ra = reader.ReadDouble();
            double dec = reader.ReadDouble();
            float parallax = reader.ReadSingle();
            float mag = reader.ReadSingle();
            float pmRa = reader.ReadSingle();
            float pmDec = reader.ReadSingle();
            
            int bytesRead = reader.Read(spectralBuffer, 0, 12);
            string spectralType = Encoding.ASCII.GetString(spectralBuffer, 0, bytesRead).TrimEnd('\0', ' ');

            entries.Add(new StarEntry(id, ra, dec, parallax, mag, pmRa, pmDec, spectralType));
        }

        return entries;
    }

    /// <summary>
    /// Writes star entries to a custom Hipparcos binary database.
    /// </summary>
    public static void WriteCatalog(string filePath, IEnumerable<StarEntry> entries)
    {
        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new BinaryWriter(stream, Encoding.ASCII);

        writer.Write(MagicBytes);
        writer.Write(CurrentVersion);

        var list = entries as List<StarEntry> ?? entries.ToList();
        writer.Write(list.Count);

        byte[] spectralBuffer = new byte[12];

        foreach (var star in list)
        {
            writer.Write(star.Id);
            writer.Write(star.RightAscension);
            writer.Write(star.Declination);
            writer.Write(star.Parallax);
            writer.Write(star.Magnitude);
            writer.Write(star.ProperMotionRa);
            writer.Write(star.ProperMotionDec);

            // Copy spectral type padded to 12 bytes
            Array.Clear(spectralBuffer, 0, 12);
            if (!string.IsNullOrEmpty(star.SpectralType))
            {
                byte[] specBytes = Encoding.ASCII.GetBytes(star.SpectralType);
                int length = System.Math.Min(specBytes.Length, 12);
                Array.Copy(specBytes, 0, spectralBuffer, 0, length);
            }
            writer.Write(spectralBuffer, 0, 12);
        }
    }
}
