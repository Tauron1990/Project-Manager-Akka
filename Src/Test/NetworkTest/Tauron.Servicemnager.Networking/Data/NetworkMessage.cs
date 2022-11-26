namespace Tauron.Servicemnager.Networking.Data;

public record NetworkMessage(string Type, byte[] Data, int Lenght)
{
    public int RealLength => Lenght == -1 ? Data.Length : Lenght;

    public static NetworkMessage Create(string type, byte[] data, int lenght = -1) => new(type, data, lenght);

    public static NetworkMessage Create(string type) => new(type, Array.Empty<byte>(), -1);
}