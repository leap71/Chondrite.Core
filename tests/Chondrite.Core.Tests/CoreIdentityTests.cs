// SPDX-License-Identifier: Apache-2.0
// Copyright 2026 LEAP 71

using System.Collections.Immutable;
using System.Text;
using Chondrite.Core;
using Leap71.Semantics;
using Xunit;

namespace Chondrite.Core.Tests;

public sealed class CoreIdentityTests
{
    private static readonly StorageType c_idTestType = new(
        new StorageObjectId(Convert.FromHexString(
            "00112233445566778899AABBCCDDEEFF00112233445566778899AABBCCDDEEFF")));

    private static StorageObject oCreate(string strPayload) => new(
        c_idTestType,
        ImmutableArray.CreateRange(Encoding.UTF8.GetBytes(strPayload)));

    [Fact]
    public void CanonicalHashKnownAnswerVectors()
    {
        Assert.Equal(
            "4773D12E2371BB935B9A0F5439B4A1C3AD3F2414B86980F8418D1CFABDFADFEF",
            oCreate("").idCalculateIdentity().ToString());
        Assert.Equal(
            "0D873A95953CAC27B2DDED5A53177048516BC4D987DCE3F164EA1AEDDAD55A1C",
            oCreate("hello").idCalculateIdentity().ToString());
    }

    [Fact]
    public void DigestRoundTripsInCanonicalByteOrder()
    {
        byte[] abyOriginal = Convert.FromHexString(
            "00112233445566778899AABBCCDDEEFF102132435465768798A9BACBDCEDFE0F");
        StorageObjectId idOriginal = new(abyOriginal);
        Span<byte> abyWritten = stackalloc byte[StorageObjectId.nByteCount];
        idOriginal.WriteBytes(abyWritten);

        Assert.True(abyOriginal.AsSpan().SequenceEqual(abyWritten));
        Assert.Equal(idOriginal, new StorageObjectId(abyWritten));
        Assert.Equal(Convert.ToHexString(abyOriginal), idOriginal.ToString());
        Assert.IsAssignableFrom<IIdentityValue>(idOriginal);
    }

    [Fact]
    public void DigestRejectsInvalidLengths()
    {
        Assert.Throws<ArgumentException>(() => new StorageObjectId(new byte[31]));
        Assert.Throws<ArgumentException>(() => new StorageObjectId(new byte[33]));
        Assert.Throws<ArgumentException>(() => default(StorageObjectId).WriteBytes(new byte[31]));
    }

    [Fact]
    public void IntrinsicSchemaIdentityIsLastByteOne()
    {
        Span<byte> abyType = stackalloc byte[StorageObjectId.nByteCount];
        StorageType.idIntrinsicSchema.WriteBytes(abyType);
        Assert.Equal(new byte[31].Concat(new byte[] { 1 }).ToArray(), abyType.ToArray());
        Assert.IsAssignableFrom<IIdentityValue>(StorageType.idIntrinsicSchema);
    }

    [Fact]
    public void StorageTypeRejectsZeroIdentity()
    {
        Assert.Throws<ArgumentException>(() => new StorageType(default));
        Assert.Throws<InvalidOperationException>(() => default(StorageType).WriteBytes(new byte[32]));
    }

    [Fact]
    public void StorageObjectIsContentAddressedByTypeAndPayload()
    {
        StorageObject oFirst = oCreate("hello");
        StorageObject oSame = oCreate("hello");
        StorageObject oOtherPayload = oCreate("world");
        StorageObject oOtherType = new(
            StorageType.idIntrinsicSchema, oFirst.abyData);

        Assert.Equal(oFirst.idCalculateIdentity(), oSame.idCalculateIdentity());
        Assert.NotEqual(oFirst.idCalculateIdentity(), oOtherPayload.idCalculateIdentity());
        Assert.NotEqual(oFirst.idCalculateIdentity(), oOtherType.idCalculateIdentity());
    }

    [Fact]
    public void StorageObjectRejectsUninitializedFields()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new StorageObject(default, ImmutableArray<byte>.Empty).idCalculateIdentity());
        Assert.Throws<InvalidOperationException>(() =>
            new StorageObject(c_idTestType, default).idCalculateIdentity());
    }

    [Fact]
    public void SchemaHashUsesIntrinsicTypePrefix()
    {
        StorageObject oSchema = new(
            StorageType.idIntrinsicSchema,
            ImmutableArray.CreateRange(new byte[] { 0x0A, 0x1B, 0x2C }));

        byte[] abyInput = new byte[32 + oSchema.abyData.Length];
        abyInput[31] = 1;
        oSchema.abyData.AsSpan().CopyTo(abyInput.AsSpan(32));
        byte[] abyExpected = System.Security.Cryptography.SHA256.HashData(abyInput);
        Assert.Equal(Convert.ToHexString(abyExpected), oSchema.idCalculateIdentity().ToString());
    }
}
