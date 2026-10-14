using System.Buffers.Binary;
using System.Text.Json;
using FarmBot.Core.Json;

namespace FarmBot.Core.Ipc;

public sealed class FrameBuffer
{
    public const int MaxPayload = 1024 * 1024;

    private byte[] _data = new byte[4096];
    private int _count;

    public void Append(ReadOnlySpan<byte> chunk)
    {
        if (chunk.Length == 0)
            return;
        Ensure(_count + chunk.Length);
        chunk.CopyTo(_data.AsSpan(_count));
        _count += chunk.Length;
    }

    public bool TryPop(out byte[] frame)
    {
        frame = Array.Empty<byte>();
        if (_count < 4)
            return false;

        var length = BinaryPrimitives.ReadInt32LittleEndian(_data);
        if (length < 0 || length > MaxPayload)
            throw new InvalidDataException($"Invalid frame length {length}.");
        if (_count < 4 + length)
            return false;

        frame = new byte[length];
        Buffer.BlockCopy(_data, 4, frame, 0, length);
        var remain = _count - 4 - length;
        if (remain > 0)
            Buffer.BlockCopy(_data, 4 + length, _data, 0, remain);
        _count = remain;
        return true;
    }

    public static byte[] Encode(ReadOnlySpan<byte> payload)
    {
        if (payload.Length > MaxPayload)
            throw new InvalidDataException("Payload exceeds the frame limit.");
        var packet = new byte[4 + payload.Length];
        BinaryPrimitives.WriteInt32LittleEndian(packet, payload.Length);
        payload.CopyTo(packet.AsSpan(4));
        return packet;
    }

    public static byte[] EncodeJson<T>(T value)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(value, JsonDefaults.Instance);
        return Encode(json);
    }

    private void Ensure(int size)
    {
        if (_data.Length >= size)
            return;
        var next = _data.Length;
        while (next < size)
            next *= 2;
        Array.Resize(ref _data, next);
    }
}
