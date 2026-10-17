#include <string>
#include <sstream>
#include <iomanip>

namespace User
{
    std::string DC_xianhuling = /*pappnase305*/XorStrTR<0x47, 12, 0x2CAE8000>("\x37\x29\x39\x3A\x25\x2D\x3E\x2B\x7C\x60\x64" + 0x2CAE8000).s;
    std::string DC_LunaValerie = /*LunaValerie*/XorStrTR<0x47, 12, 0x2CAE8000>("\x0B\x3D\x27\x2B\x1D\x2D\x21\x2B\x3D\x39\x34" + 0x2CAE8000).s;
    std::string DC_Ravageravage = /*mamibeyhz43*/XorStrTR<0x47, 12, 0x2CAE8000>("\x2A\x29\x24\x23\x29\x29\x34\x26\x35\x64\x62" + 0x2CAE8000).s;
    std::string DC_s_ako = /*SaykoTr*/XorStrTR<0x47, 8, 0x2CAE8000>("\x14\x29\x30\x21\x24\x18\x3F" + 0x2CAE8000).s;
    std::string DC_BaranKılıc= /*Baran*/XorStrTR<0x47, 6, 0x2CAE8000>("\x05\x29\x3B\x2B\x25" + 0x2CAE8000).s;
    std::string CheckAllUser = /*https://pastebin.com/raw/kMBJKFMy*/XorStrTR<0x47, 34, 0x2CAE8000>("\x2F\x3C\x3D\x3A\x38\x76\x62\x61\x3F\x31\x22\x26\x36\x36\x3C\x38\x79\x3B\x36\x37\x74\x2E\x3C\x29\x70\x0B\x2C\x20\x29\x2F\x23\x2B\x1E" + 0x2CAE8000).s;

    static std::string s_model = "";
    static std::string s_serial = "";
    static std::string s_size = "";
}

int GetPhysicalDriveNumberFromVolume(char driveLetter) {
    std::string volumePath = "\\\\.\\";
    volumePath += driveLetter;
    volumePath += ":";

    HANDLE hVol = CreateFileA(volumePath.c_str(),
        0,
        FILE_SHARE_READ | FILE_SHARE_WRITE,
        nullptr,
        OPEN_EXISTING,
        0,
        nullptr);

    if (hVol == INVALID_HANDLE_VALUE)
        return -1;

    DWORD bytesReturned = 0;
    STORAGE_DEVICE_NUMBER sdn{};
    BOOL ok = DeviceIoControl(hVol,
        IOCTL_STORAGE_GET_DEVICE_NUMBER,
        nullptr, 0,
        &sdn, sizeof(sdn),
        &bytesReturned,
        nullptr);
    CloseHandle(hVol);

    if (!ok)
        return -1;

    return static_cast<int>(sdn.DeviceNumber);
}

std::string ReadSerialFromDesc(const STORAGE_DEVICE_DESCRIPTOR* desc) {
    if (!desc || desc->SerialNumberOffset == 0 || desc->SerialNumberOffset >= desc->Size)
        return "";
    const char* p = reinterpret_cast<const char*>(desc) + desc->SerialNumberOffset;
    std::string s(p);
    s.erase(std::remove_if(s.begin(), s.end(),
        [](char c) { return c == ' ' || c == '\0' || c < 32; }),
        s.end());
    return s;
}

std::string ReadModelFromDesc(const STORAGE_DEVICE_DESCRIPTOR* desc) {
    if (!desc || desc->ProductIdOffset == 0 || desc->ProductIdOffset >= desc->Size)
        return "";
    const char* p = reinterpret_cast<const char*>(desc) + desc->ProductIdOffset;
    std::string s(p);
    s.erase(std::remove_if(s.begin(), s.end(),
        [](char c) { return c == ' ' || c == '\0' || c < 32; }),
        s.end());
    return s;
}

std::string HumanSize(uint64_t bytes) {
    const char* units[] = { "B", "KB", "MB", "GB", "TB" };
    double v = static_cast<double>(bytes);
    int u = 0;
    while (v >= 1024.0 && u < 4) {
        v /= 1024.0;
        ++u;
    }
    std::ostringstream ss;
    ss << std::fixed << std::setprecision(2) << v << " " << units[u];
    return ss.str();
}

bool QueryPhysicalDriveDescriptor(int physicalNumber, std::string& outModel, std::string& outSerial, std::string& outSize) {
    std::string path = "\\\\.\\PhysicalDrive" + std::to_string(physicalNumber);
    HANDLE hDisk = CreateFileA(path.c_str(),
        GENERIC_READ,
        FILE_SHARE_READ | FILE_SHARE_WRITE,
        nullptr, OPEN_EXISTING, 0, nullptr);
    if (hDisk == INVALID_HANDLE_VALUE)
        return false;

    STORAGE_PROPERTY_QUERY query{};
    query.PropertyId = StorageDeviceProperty;
    query.QueryType = PropertyStandardQuery;

    BYTE buffer[1024] = { 0 };
    DWORD bytesReturned = 0;
    if (!DeviceIoControl(hDisk,
        IOCTL_STORAGE_QUERY_PROPERTY,
        &query, sizeof(query),
        buffer, sizeof(buffer),
        &bytesReturned,
        nullptr)) {
        CloseHandle(hDisk);
        return false;
    }

    STORAGE_DEVICE_DESCRIPTOR* desc = reinterpret_cast<STORAGE_DEVICE_DESCRIPTOR*>(buffer);
    outModel = ReadModelFromDesc(desc);
    outSerial = ReadSerialFromDesc(desc);
    DISK_GEOMETRY_EX geom{};
    if (DeviceIoControl(hDisk, IOCTL_DISK_GET_DRIVE_GEOMETRY_EX,
        nullptr, 0,
        &geom, sizeof(geom),
        &bytesReturned, nullptr)) {
        outSize = HumanSize(geom.DiskSize.QuadPart);
    }
    else {
        outSize = "Unknown";
    }

    CloseHandle(hDisk);
    return true;
}