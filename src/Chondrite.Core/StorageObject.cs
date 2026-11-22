// SPDX-License-Identifier: Apache-2.0
// Copyright 2026 LEAP 71

using System.Collections.Immutable;
using System.Security.Cryptography;

namespace Chondrite.Core;

/// <summary>
/// An immutable, typed storage object whose payload is opaque to the storage system.
/// </summary>
/// <param name="idType">Identity of the exact persisted schema for the payload.</param>
/// <param name="abyData">Immutable payload bytes. A domain may place its own creation time or provenance here.</param>
/// <remarks>
/// Both fields contribute to the SHA-256 <see cref="StorageObjectId"/>. The canonical
/// representation consists of 32 type-identity bytes followed by the payload bytes,
/// with no timestamp, length prefix, or padding. Re-publishing the same bytes with
/// the same type identity is therefore idempotent across nodes.
///
/// The payload must be an initialized <see cref="ImmutableArray{T}"/>. Callers
/// constructing it from mutable buffers must copy them. Generated record equality
/// is not bytewise payload equality; use <see cref="idCalculateIdentity"/> to compare identities.
/// Storage has no opinion on schema semantics, provenance, or domain relationships.
/// </remarks>
public sealed record StorageObject(
    StorageType          idType,
    ImmutableArray<byte> abyData)
{
    /// <summary>
    /// Calculates SHA-256 of the canonical type identity followed by the payload bytes.
    /// </summary>
    public StorageObjectId idCalculateIdentity()
    {
        if (abyData.IsDefault)
            throw new InvalidOperationException("Storage payload is uninitialized.");
        if (idType == default)
            throw new InvalidOperationException("Storage type is uninitialized.");

        Span<byte> abyType = stackalloc byte[StorageObjectId.nByteCount];
        idType.WriteBytes(abyType);

        using IncrementalHash oHash =
            IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        oHash.AppendData(abyType);
        oHash.AppendData(abyData.AsSpan());

        Span<byte> abyHash = stackalloc byte[StorageObjectId.nByteCount];
        oHash.GetHashAndReset(abyHash);
        return new StorageObjectId(abyHash);
    }
}
