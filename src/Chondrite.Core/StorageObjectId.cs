// SPDX-License-Identifier: Apache-2.0
// Copyright 2026 LEAP 71

using System.Buffers.Binary;
using Leap71.Semantics;

namespace Chondrite.Core;

/// <summary>
/// Identifies an immutable <see cref="StorageObject"/> by its 256-bit SHA-256 digest.
/// </summary>
/// <remarks>
/// Four 64-bit values provide allocation-free storage and value equality. The public byte
/// representation is always the canonical 32-byte digest, independent of native endianness.
/// This type represents a digest; it does not calculate SHA-256 itself.
/// <c>default(StorageObjectId)</c> is an all-zero digest. The schema layer reserves zero as an
/// invalid type identity; use <see cref="Nullable{T}"/> for an optional identity.
/// </remarks>
public readonly record struct StorageObjectId : IIdentityValue
{
    /// <summary>The fixed size of a SHA-256 digest in bytes.</summary>
    public const int nByteCount = 32;

    private readonly ulong m_nA;
    private readonly ulong m_nB;
    private readonly ulong m_nC;
    private readonly ulong m_nD;

    /// <summary>Creates an identity from a canonical 32-byte SHA-256 digest.</summary>
    /// <param name="abyHash">Digest bytes in their standard byte order.</param>
    /// <exception cref="ArgumentException">The supplied digest is not exactly 32 bytes long.</exception>
    /// <remarks>Each eight-byte group is interpreted as a big-endian unsigned integer.</remarks>
    public StorageObjectId(ReadOnlySpan<byte> abyHash)
    {
        if (abyHash.Length != nByteCount)
            throw new ArgumentException("SHA-256 hash must be 32 bytes.");

        m_nA = BinaryPrimitives.ReadUInt64BigEndian(abyHash[0..8]);
        m_nB = BinaryPrimitives.ReadUInt64BigEndian(abyHash[8..16]);
        m_nC = BinaryPrimitives.ReadUInt64BigEndian(abyHash[16..24]);
        m_nD = BinaryPrimitives.ReadUInt64BigEndian(abyHash[24..32]);
    }

    /// <summary>Writes the canonical SHA-256 digest into a caller-provided buffer.</summary>
    /// <param name="abyTarget">Destination with at least <see cref="nByteCount"/> bytes available.</param>
    /// <exception cref="ArgumentException">The destination is too small.</exception>
    /// <remarks>Only the first 32 bytes are written. No heap allocation is required.</remarks>
    public void WriteBytes(Span<byte> abyTarget)
    {
        if (abyTarget.Length < nByteCount)
            throw new ArgumentException("Target buffer too small.");

        BinaryPrimitives.WriteUInt64BigEndian(abyTarget[0..8], m_nA);
        BinaryPrimitives.WriteUInt64BigEndian(abyTarget[8..16], m_nB);
        BinaryPrimitives.WriteUInt64BigEndian(abyTarget[16..24], m_nC);
        BinaryPrimitives.WriteUInt64BigEndian(abyTarget[24..32], m_nD);
    }

    /// <summary>Formats the 32-byte digest as 64 uppercase hexadecimal characters.</summary>
    public override string ToString()
    {
        Span<byte> abyHash = stackalloc byte[nByteCount];
        WriteBytes(abyHash);
        return Convert.ToHexString(abyHash);
    }
}
