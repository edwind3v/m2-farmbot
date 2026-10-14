using System.Text;
using FarmBot.Core.Combat;

namespace FarmBot.Tests;

public class PlayerActionLayoutTests
{
    private const string ReleaseClient = @"E:\TeraBoxDownload\1.Client\1.metin2client.exe";

    [Fact]
    public void RecoversReleaseSetTargetWithoutUsingDebugAddresses()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var layout = PlayerActionLayoutRecoverer.Recover(ReleaseClient);
        var dump = $"singleton=0x{layout.PlayerSingletonRva:X} setTarget=0x{layout.SetTargetRva:X} getTarget=0x{layout.GetTargetVidRva:X} attackKey=0x{layout.SetAttackKeyStateRva:X} combo=0x{layout.ComboAttackRva:X} pick=0x{layout.PickCloseItemRva:X} targetDisp=0x{layout.TargetVidDisplacement:X} thisAdj=0x{layout.SetTargetThisAdjustment:X} slot=0x{layout.SetTargetVtableSlot:X} prologue={layout.SetTargetPrologue} points=+0x{layout.StatusPointsDisplacement:X} name=+0x{layout.PlayerNameDisplacement:X} xor=0x{layout.PointXorMask:X}\n" +
                   string.Join("\n", layout.Evidence);
        Directory.CreateDirectory(@"C:\Temp");
        File.WriteAllText(@"C:\Temp\playeraction.txt", dump);

        Assert.NotEqual(0u, layout.PreferredImageBase);
        Assert.True(layout.HasSetTarget, dump);
        Assert.True(layout.PlayerSingletonRva is > 0x1000 and < 0x8000000, dump);
        Assert.Equal(4u, layout.SetTargetThisAdjustment);
        Assert.Equal(0x60u, layout.SetTargetVtableSlot);
        Assert.NotEqual(0x4EA2F0u, layout.SetTargetRva);
        Assert.Contains(layout.Evidence, item => item.Contains("thiscall", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(0x1036Cu, layout.StatusPointsDisplacement);
        Assert.Equal(0x14u, layout.PlayerNameDisplacement);
        Assert.Equal(0xE73AC1DAu, layout.PointXorMask);
        Assert.Equal(1, layout.PointLevel);
        Assert.Equal(3, layout.PointExp);
        Assert.Equal(4, layout.PointNextExp);
        Assert.Equal(5, layout.PointHp);
        Assert.Equal(6, layout.PointMaxHp);
        Assert.Equal(7, layout.PointSp);
        Assert.Equal(8, layout.PointMaxSp);
        Assert.Equal(0xD1280u, layout.PickCloseItemRva);
    }

    [Fact]
    public void ReleaseImageHasSendAttackPacketUsedByRangedShots()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var bytes = File.ReadAllBytes(ReleaseClient);
        Assert.Contains("Send Battle Attack Packet Error", Encoding.ASCII.GetString(bytes));
        var eLfanew = BitConverter.ToInt32(bytes, 0x3C);
        var imageBase = BitConverter.ToUInt32(bytes, eLfanew + 24 + 28);
        var sections = BitConverter.ToUInt16(bytes, eLfanew + 6);
        var optional = BitConverter.ToUInt16(bytes, eLfanew + 20);
        var off = eLfanew + 24 + optional;
        int file = -1;
        for (var i = 0; i < sections; i++)
        {
            var at = off + i * 40;
            var va = BitConverter.ToUInt32(bytes, at + 12);
            var raw = BitConverter.ToUInt32(bytes, at + 20);
            var rawSize = BitConverter.ToUInt32(bytes, at + 16);
            if (0x134BE0u >= va && 0x134BE0u < va + rawSize)
            {
                file = (int)(raw + (0x134BE0u - va));
                break;
            }
        }

        Assert.True(file >= 0 && file + 8 <= bytes.Length);
        Assert.Equal(new byte[] { 0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x08, 0x56, 0x8B }, bytes[file..(file + 8)]);
        Assert.Contains("CPythonNetworkStream::SendCharacterStatePacket", Encoding.ASCII.GetString(bytes));
        Assert.Equal(0x400000u, imageBase);
    }

    [Fact]
    public void ReleaseImageHasSendItemPickUpPacketUsedByFastPickup()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var bytes = File.ReadAllBytes(ReleaseClient);
        Assert.Contains("SendItemPickUpPacket", Encoding.ASCII.GetString(bytes));
        var eLfanew = BitConverter.ToInt32(bytes, 0x3C);
        var sections = BitConverter.ToUInt16(bytes, eLfanew + 6);
        var optional = BitConverter.ToUInt16(bytes, eLfanew + 20);
        var off = eLfanew + 24 + optional;
        int file = -1;
        for (var i = 0; i < sections; i++)
        {
            var at = off + i * 40;
            var va = BitConverter.ToUInt32(bytes, at + 12);
            var raw = BitConverter.ToUInt32(bytes, at + 20);
            var rawSize = BitConverter.ToUInt32(bytes, at + 16);
            if (0x13B690u >= va && 0x13B690u < va + rawSize)
            {
                file = (int)(raw + (0x13B690u - va));
                break;
            }
        }

        Assert.True(file >= 0 && file + 8 <= bytes.Length);
        Assert.Equal(new byte[] { 0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x08, 0x56, 0x8B }, bytes[file..(file + 8)]);
    }

    [Fact]
    public void GetCloseItemNegatesGroundItemYBeforeComparingToPlayerPixels()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var bytes = File.ReadAllBytes(ReleaseClient);
        var eLfanew = BitConverter.ToInt32(bytes, 0x3C);
        var sections = BitConverter.ToUInt16(bytes, eLfanew + 6);
        var optional = BitConverter.ToUInt16(bytes, eLfanew + 20);
        var off = eLfanew + 24 + optional;
        int file = -1;
        for (var i = 0; i < sections; i++)
        {
            var at = off + i * 40;
            var va = BitConverter.ToUInt32(bytes, at + 12);
            var raw = BitConverter.ToUInt32(bytes, at + 20);
            var rawSize = BitConverter.ToUInt32(bytes, at + 16);
            if (0xBB480u >= va && 0xBB480u < va + rawSize)
            {
                file = (int)(raw + (0xBB480u - va));
                break;
            }
        }

        Assert.True(file >= 0 && file + 0xC0 <= bytes.Length);
        var fn = bytes[file..(file + 0xC0)];
        Assert.True(fn.AsSpan().IndexOf(new byte[] { 0xF3, 0x0F, 0x10, 0x40, 0x0C }) >= 0);
        Assert.True(fn.AsSpan().IndexOf(new byte[] { 0x0F, 0x57, 0x05 }) >= 0);
    }

    [Fact]
    public void GetCloseMoneySelectsYangFlagThenPickCloseMoneyUsesIt()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var bytes = File.ReadAllBytes(ReleaseClient);
        var eLfanew = BitConverter.ToInt32(bytes, 0x3C);
        var sections = BitConverter.ToUInt16(bytes, eLfanew + 6);
        var optional = BitConverter.ToUInt16(bytes, eLfanew + 20);
        var off = eLfanew + 24 + optional;
        int moneyFile = -1;
        int pickFile = -1;
        for (var i = 0; i < sections; i++)
        {
            var at = off + i * 40;
            var va = BitConverter.ToUInt32(bytes, at + 12);
            var raw = BitConverter.ToUInt32(bytes, at + 20);
            var rawSize = BitConverter.ToUInt32(bytes, at + 16);
            if (0xBB580u >= va && 0xBB580u < va + rawSize)
                moneyFile = (int)(raw + (0xBB580u - va));
            if (0xD12F0u >= va && 0xD12F0u < va + rawSize)
                pickFile = (int)(raw + (0xD12F0u - va));
        }

        Assert.True(moneyFile >= 0 && moneyFile + 0x80 <= bytes.Length);
        Assert.True(pickFile >= 0 && pickFile + 0x50 <= bytes.Length);
        var money = bytes[moneyFile..(moneyFile + 0x80)];
        Assert.True(money.AsSpan().IndexOf(new byte[] { 0x83, 0x78, 0x04, 0x01 }) >= 0);
        var pick = bytes[pickFile..(pickFile + 0x50)];
        Assert.Equal(new byte[] { 0x55, 0x8B, 0xEC, 0x83, 0xEC, 0x14 }, pick[..6]);
        var rel = BitConverter.ToInt32(pick, 0xD1337 - 0xD12F0 + 1);
        Assert.Equal(0xBB580, 0xD1337 + 5 + rel);
    }

    [Fact]
    public void SendClickLooksUpOwnerNameAtGroundInstancePlus2FC()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var bytes = File.ReadAllBytes(ReleaseClient);
        var eLfanew = BitConverter.ToInt32(bytes, 0x3C);
        var sections = BitConverter.ToUInt16(bytes, eLfanew + 6);
        var optional = BitConverter.ToUInt16(bytes, eLfanew + 20);
        var off = eLfanew + 24 + optional;
        int file = -1;
        for (var i = 0; i < sections; i++)
        {
            var at = off + i * 40;
            var va = BitConverter.ToUInt32(bytes, at + 12);
            var raw = BitConverter.ToUInt32(bytes, at + 20);
            var rawSize = BitConverter.ToUInt32(bytes, at + 16);
            if (0xBB7E0u >= va && 0xBB7E0u < va + rawSize)
            {
                file = (int)(raw + (0xBB7E0u - va));
                break;
            }
        }

        Assert.True(file >= 0 && file + 0x50 <= bytes.Length);
        var fn = bytes[file..(file + 0x50)];
        Assert.True(fn.AsSpan().IndexOf(new byte[] { 0x81, 0xC1, 0xFC, 0x02, 0x00, 0x00 }) >= 0);
    }

    [Fact]
    public void RecoversMiniMapAtlasBindings()
    {
        Assert.True(File.Exists(ReleaseClient), ReleaseClient);
        var mini = PlayerActionLayoutRecoverer.RecoverMiniMap(ReleaseClient);
        var dump = $"singleton=0x{mini.SingletonRva:X} show=0x{mini.ShowAtlasRva:X} hide=0x{mini.HideAtlasRva:X} load=0x{mini.LoadAtlasRva:X} update=0x{mini.UpdateAtlasRva:X} render=0x{mini.RenderAtlasRva:X} isShow=0x{mini.IsShowAtlasRva:X}\n" +
                   string.Join("\n", mini.Evidence);
        Directory.CreateDirectory(@"C:\Temp");
        File.WriteAllText(@"C:\Temp\minimap-layout.txt", dump);
        Assert.Equal(0x482BA8u, mini.SingletonRva);
        Assert.Equal(0xF9980u, mini.LoadAtlasRva);
        Assert.Equal(0xFB7B0u, mini.ShowAtlasRva);
        Assert.Equal(0xF9960u, mini.HideAtlasRva);
    }

    [Fact]
    public void RecoversArthionPlayerWhenPresent()
    {
        const string arthion = @"E:\Arthion\game\Arthion.exe";
        if (!File.Exists(arthion))
            return;

        var player = PlayerActionLayoutRecoverer.Recover(arthion);
        var entity = FarmBot.Core.Entities.ClientReleaseLayout.Recover(arthion);
        var login = PlayerActionLayoutRecoverer.RecoverLogin(arthion);
        var singleton = player.PlayerSingletonRva != 0 ? player.PlayerSingletonRva : entity.SingletonRva - 4;
        var dump = $"player=0x{player.PlayerSingletonRva:X} fallback=0x{singleton:X} charMgr=0x{entity.SingletonRva:X} setTarget=0x{player.SetTargetRva:X} setTargetSlot=0x{player.SetTargetVtableSlot:X} thisAdj=0x{player.SetTargetThisAdjustment:X} attackKey=0x{player.SetAttackKeyStateRva:X} newAttack=0x{player.NewAttackRva:X} combo=0x{player.ComboAttackRva:X} sendAttack=0x{player.SendAttackPacketRva:X} sendState=0x{player.SendStatePacketRva:X} setAuto=0x{player.SetAutoRva:X} reserve=0x{player.ReserveClickRva:X} moveTo=0x{player.MoveToDestRva:X} movePixel=0x{player.MoveToPixelRva:X} getMain=0x{player.GetMainActorRva:X} pressActor=0x{player.OnPressActorRva:X} chase=0x{player.ChaseActorRva:X} approachInst=0x{player.ApproachInstRva:X} inRange=0x{player.InRangeRva:X} playerUpdate=0x{player.PlayerUpdateRva:X} map=+0x{entity.MapDisplacement:X} getStatus=0x{player.GetStatusRva:X} sendPickup=0x{player.SendPickupPacketRva:X} item=0x{player.ItemSingletonRva:X} getClose=0x{player.GetCloseItemRva:X} getItemIndex=0x{player.GetItemIndexRva:X} getItemCount=0x{player.GetItemCountRva:X} selectItem=0x{player.SelectItemRva:X} getItemName=0x{player.GetItemNameRva:X} getItemType=0x{player.GetItemTypeRva:X} getItemSub=0x{player.GetItemSubTypeRva:X} getAffect=0x{player.GetAffectRva:X} getValue=0x{player.GetValueRva:X} points=+0x{player.StatusPointsDisplacement:X} xor=0x{player.PointXorMask:X} name=+0x{player.PlayerNameDisplacement:X} loginId=+0x{login.LoginIdDisplacement:X}\n" +
                   string.Join("\n", player.Evidence);
        Directory.CreateDirectory(@"C:\Temp");
        File.WriteAllText(@"C:\Temp\arthion-player.txt", dump);
        Assert.True(singleton is > 0x1000 and < 0x8000000, dump);
        Assert.True(player.SetTargetVtableSlot == 0 || player.SetTargetThisAdjustment != 0, dump);
        Assert.NotEqual(0u, player.ComboAttackRva);
        Assert.NotEqual(0u, player.ReserveClickRva);
        Assert.NotEqual(0u, player.SendAttackPacketRva);
        Assert.NotEqual(0u, player.MoveToDestRva);
        Assert.NotEqual(0u, player.MoveToPixelRva);
        Assert.NotEqual(0u, player.GetMainActorRva);
        Assert.NotEqual(0u, player.ChaseActorRva);
        Assert.NotEqual(0u, player.ApproachInstRva);
        Assert.NotEqual(0u, player.PlayerUpdateRva);
        Assert.NotEqual(0u, player.GetPixelRva);
        Assert.NotEqual(0u, player.InRangeRva);
        Assert.Equal(0x26F7E0u, player.InRangeRva);
        Assert.Equal(0x3909F0u, player.OnPressActorRva);
        Assert.NotEqual(0x608290u, player.MoveToDestRva);
        Assert.NotEqual(0u, player.GetItemIndexRva);
        Assert.NotEqual(0x608290u, player.GetItemIndexRva);
        Assert.NotEqual(0u, player.GetItemCountRva);
        Assert.NotEqual(0x608290u, player.GetItemNameRva);
        Assert.Equal(0x369550u, player.SendItemUsePacketRva);
        Assert.NotEqual(0x37D360u, player.SendItemUsePacketRva);
    }
}
