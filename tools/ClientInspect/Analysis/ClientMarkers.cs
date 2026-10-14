namespace ClientInspect.Analysis;

public static class ClientMarkers
{
    public static IReadOnlyList<NamedMarker> Scan(string path)
    {
        var catalog = new (string Name, string Role)[]
        {
            (".?AVCPythonCharacterManager@@", "RTTI: nearby-instance table owner"),
            (".?AVCInstanceBase@@", "RTTI: one visible character / monster / stone"),
            (".?AVIAbstractCharacterManager@@", "RTTI: character-manager interface"),
            (".?AVCNetworkActorManager@@", "RTTI: server-driven actor list"),
            (".?AVCActorInstance@@", "RTTI: rendered actor"),
            (".?AVCPythonNetworkStream@@", "RTTI: client network stream"),
            ("CPythonCharacterManager::CreateInstance VID[%d] Race[%d]", "create instance by VID + race"),
            ("CPythonCharacterManager::CreateInstance: VID[%d] - ALREADY EXIST", "VID is the instance key"),
            ("DeleteCharacterInstance: no vid by %d", "delete instance by VID"),
            ("SelectCharacterInstance: no vid by %d", "select instance by VID"),
            ("CNetworkActorManager::__AppendCharacterManagerActor(%d, %s)", "network actor appended with VID + name"),
            ("CNetworkActorManager::RemoveActor(dwVID=%d) - NOT EXIST VID", "network actor removed by VID"),
            ("CNetworkActorManager::MoveActor(dwVID=%d) - NOT EXIST VID", "network actor moved by VID"),
            ("CNetworkActorManager::UpdateActor(dwVID=%d) - NOT EXIST VID", "network actor updated by VID"),
            ("TPacketGCCharacterAdditionalInfo name=%s vid=%d race=%d Error", "name, VID, race arrive together"),
            ("TPacketGCCharacterAdd2 is packet that doesn't write.", "second character-add packet form"),
            ("RecvCharacterMovePacket", "move packet handler"),
            ("RecvCharacterDeletePacket", "delete packet handler"),
            ("GetVID", "Python/C++ VID getter"),
            ("GetNameByVID", "name lookup by VID"),
            ("GetPixelPosition", "world/pixel position getter"),
            ("GetInstanceType", "instance type getter"),
            ("INSTANCE_TYPE_PLAYER", "instance type: player"),
            ("INSTANCE_TYPE_NPC", "instance type: NPC"),
            ("INSTANCE_TYPE_ENEMY", "instance type: enemy / monster"),
            ("INSTANCE_TYPE_BUILDING", "instance type: building"),
            ("INSTANCE_TYPE_OBJECT", "instance type: object"),
            ("IsEnemy", "enemy predicate"),
            ("IsNPC", "NPC predicate"),
            ("IsDead", "dead predicate"),
            ("HasInstance", "VID present in manager"),
            ("GetMainVID", "local player VID"),
            ("GetMainCharacterIndex", "local player VID (Python player module)"),
            ("GetMainCharacterPosition", "local player x,y,z"),
            ("GetPickedVID", "currently picked VID"),
            ("GetTargetVID", "current target VID"),
            ("GetRace", "race / vnum getter"),
            ("GetMaxHPByVID", "max HP by VID"),
            ("StoneDetect [VID:%d] [Distance:%d] [Angle:%d->%f]", "Metin stone detector uses VID + distance"),
            (".?AV?$CDynamicPool@USGroundItemInstance@CPythonItem@@@@", "RTTI: ground item / drop pool")
        };

        var present = new HashSet<string>(PeImage.FindAscii(path, catalog.Select(item => item.Name)));
        return catalog
            .Where(item => present.Contains(item.Name))
            .Select(item => new NamedMarker(item.Name, "disk", item.Role))
            .ToList();
    }

    public static IReadOnlyList<string> StructureNotes(IReadOnlyList<NamedMarker> markers)
    {
        var names = markers.Select(marker => marker.Name).ToHashSet(StringComparer.Ordinal);
        var notes = new List<string>
        {
            "Do not invent CInstanceBase field offsets. Debug-confirmed displacements are listed in the Phase 3 facts; the live release process was not read.",
            "Nearby entities live as CInstanceBase objects owned by CPythonCharacterManager in m_kAliveInstMap, keyed by VID.",
            "CNetworkActorManager is the server-fed actor cache (m_kNetActorDict) that appends, moves, updates, and removes those instances."
        };

        if (names.Contains("INSTANCE_TYPE_ENEMY"))
            notes.Add("Python exports PLAYER=6, NPC=1, ENEMY=0, BUILDING=5, OBJECT=10. C++ also has TYPE_STONE=2; that name is not exported to Python.");
        if (names.Contains("StoneDetect [VID:%d] [Distance:%d] [Angle:%d->%f]"))
            notes.Add("StoneDetect is a server command (VID, distance, angle) that plays a compass effect. Stones are classified as TYPE_STONE on the actor, not inside StoneDetect.");
        if (names.Contains("GetPixelPosition") && names.Contains("GetMainCharacterPosition"))
            notes.Add("Positions are TPixelPosition = three floats (x, y=-graphicY, z) returned as Python format fff.");
        if (names.Contains("TPacketGCCharacterAdditionalInfo name=%s vid=%d race=%d Error"))
            notes.Add("The server fills name, VID, and race before the instance is fully usable. That is how the client learns who is nearby.");
        if (names.Contains(".?AV?$CDynamicPool@USGroundItemInstance@CPythonItem@@@@"))
            notes.Add("Ground loot is a separate CPythonItem::SGroundItemInstance pool, not a CInstanceBase.");

        notes.Add("Ordinary user-mode ReadProcessMemory of these objects is only valid if OpenProcess(PROCESS_VM_READ) is granted. This tool does not walk instances and does not alter process protection.");
        return notes;
    }
}
