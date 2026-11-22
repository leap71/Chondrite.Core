// SPDX-License-Identifier: Apache-2.0
// Copyright 2026 LEAP 71

using Leap71.Semantics;

namespace Chondrite.Core;

/// <summary>
/// Strongly typed reference to the immutable StorageObject defining a persistence schema.
/// </summary>
/// <remarks>
/// A StorageType has exactly the same canonical 32-byte representation as its underlying
/// StorageObjectId. Construction does not retrieve or validate the referenced schema.
/// Schema existence and interpretation are responsibilities of the typed persistence layer.
/// The default value is invalid, as for other uninitialized identity values.
/// </remarks>
public readonly record struct StorageType : IIdentityValue
{
    /// <summary>Identity of the StorageObject containing the schema definition.</summary>
    public StorageObjectId idSchema { get; }

    /// <summary>Reserved intrinsic type for published schema definitions (identity 1).</summary>
    public static StorageType idIntrinsicSchema { get; } = new(
        new StorageObjectId(Convert.FromHexString(
            "0000000000000000000000000000000000000000000000000000000000000001")));

    /// <summary>Explicitly identifies an existing object identity as a schema identity.</summary>
    /// <exception cref="ArgumentException">The underlying identity is uninitialized.</exception>
    public StorageType(StorageObjectId idSchema)
    {
        if (idSchema == default)
            throw new ArgumentException("Schema identity cannot be zero.", nameof(idSchema));
        this.idSchema = idSchema;
    }

    /// <summary>Writes the canonical 32 bytes of the referenced schema identity.</summary>
    public void WriteBytes(Span<byte> abyTarget)
    {
        if (this == default)
            throw new InvalidOperationException("Uninitialized storage type.");
        idSchema.WriteBytes(abyTarget);
    }

    /// <summary>Formats the referenced schema hash as hexadecimal.</summary>
    public override string ToString() => idSchema.ToString();
}
