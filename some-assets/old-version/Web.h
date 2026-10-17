#include <iostream>
#include <fstream>
#include <string>
#include <Windows.h>
#include <wininet.h>
#include <regex>
#include <filesystem>
#include <iomanip>

#pragma comment(lib, "wininet.lib")


static inline std::string trim(const std::string& s) {
    size_t start = 0;
    while (start < s.size() && (s[start] == ' ' || s[start] == '\t' || s[start] == '\r' || s[start] == '\n')) ++start;
    size_t end = s.size();
    while (end > start && (s[end - 1] == ' ' || s[end - 1] == '\t' || s[end - 1] == '\r' || s[end - 1] == '\n' || s[end - 1] == '.')) --end;
    return s.substr(start, end - start);
}

std::string replaceAll(std::string subject, const std::string& search, const std::string& replace) {
    size_t pos = 0;
    while ((pos = subject.find(search, pos)) != std::string::npos) {
        subject.replace(pos, search.length(), replace);
        pos += replace.length();
    }
    return subject;
}

std::string DownloadString(const std::string& URL) {
    HINTERNET interwebs = InternetOpenA("Mozilla/5.0", INTERNET_OPEN_TYPE_DIRECT, NULL, NULL, 0);
    std::string result;
    if (interwebs) {
        HINTERNET urlFile = InternetOpenUrlA(interwebs, URL.c_str(), NULL, 0, INTERNET_FLAG_RELOAD, 0);
        if (urlFile) {
            char buffer[2048];
            DWORD bytesRead = 0;
            do {
                InternetReadFile(urlFile, buffer, sizeof(buffer), &bytesRead);
                if (bytesRead > 0) result.append(buffer, bytesRead);
            } while (bytesRead);
            InternetCloseHandle(urlFile);
        }
        InternetCloseHandle(interwebs);
    }
    return replaceAll(result, "|n", "\r\n");
}

std::unordered_map<std::string, std::string> parseUsers(const std::string& data) {
    std::unordered_map<std::string, std::string> map;
    std::istringstream iss(data);
    std::string line;
    while (std::getline(iss, line)) {
        std::string orig = line;
        line = trim(line);
        if (line.empty()) continue;
        size_t colon = line.find(':');
        if (colon == std::string::npos) continue;
        std::string user = trim(line.substr(0, colon));
        std::string serial = trim(line.substr(colon + 1));
        if (!user.empty() && !serial.empty()) {
            map[user] = serial;
        }
    }
    return map;
}

bool checkLogin(const std::string& username, const std::string& serial, const std::string& pastebinURL) {
    std::string data = DownloadString(pastebinURL);
    if (data.empty()) {
        return false;
    }
    auto users = parseUsers(data);
    auto it = users.find(username);
    if (it == users.end()) return false;
    return (it->second == serial);
}

//std::string replaceAll(std::string subject, const std::string& search, const std::string& replace) {
//    size_t pos = 0;
//    while ((pos = subject.find(search, pos)) != std::string::npos) {
//        subject.replace(pos, search.length(), replace);
//        pos += replace.length();
//    }
//    return subject;
//}
//
//
//std::string DownloadString(std::string URL) {
//    HINTERNET interwebs = InternetOpenA("Mozilla/5.0", INTERNET_OPEN_TYPE_DIRECT, NULL, NULL, NULL);
//    HINTERNET urlFile;
//    std::string rtn;
//    if (interwebs) {
//        urlFile = InternetOpenUrlA(interwebs, URL.c_str(), NULL, NULL, NULL, NULL);
//        if (urlFile) {
//            char buffer[2000];
//            DWORD bytesRead;
//            do {
//                InternetReadFile(urlFile, buffer, 2000, &bytesRead);
//                rtn.append(buffer, bytesRead);
//                memset(buffer, 0, 2000);
//            } while (bytesRead);
//            InternetCloseHandle(interwebs);
//            InternetCloseHandle(urlFile);
//            std::string p = replaceAll(rtn, "|n", "\r\n");
//            return p;
//        }
//        else {
//            std::cerr << "Failed to open URL." << std::endl;
//        }
//    }
//    InternetCloseHandle(interwebs);
//    return rtn;
//}

std::string ExtractURLForGame(const std::string& text, const std::string& gameTitle) {
    size_t pos = text.find(gameTitle);
    if (pos != std::string::npos) {
        size_t startPos = text.find("https://cdn.discordapp.com", pos);
        if (startPos != std::string::npos) {
            size_t endPos = text.find_first_of(" \n", startPos);
            if (endPos == std::string::npos) {
                endPos = text.length();
            }
            return text.substr(startPos, endPos - startPos);
        }
    }
    return "";
}


bool DownloadFile(const std::string& url, const std::string& savePath) {
    HINTERNET hInternet = InternetOpenA("UserAgent", INTERNET_OPEN_TYPE_DIRECT, NULL, NULL, 0);
    if (!hInternet) {
        std::cerr << "InternetOpen failed" << std::endl;
        return false;
    }

    HINTERNET hUrl = InternetOpenUrlA(hInternet, url.c_str(), NULL, 0, INTERNET_FLAG_PRAGMA_NOCACHE | INTERNET_FLAG_KEEP_CONNECTION, 0);
    if (!hUrl) {
        std::cerr << "InternetOpenUrl failed" << std::endl;
        InternetCloseHandle(hInternet);
        return false;
    }

    std::ofstream outFile(savePath, std::ios::binary);
    if (!outFile.is_open()) {
        std::cerr << "Failed to open file for writing" << std::endl;
        InternetCloseHandle(hUrl);
        InternetCloseHandle(hInternet);
        return false;
    }

    DWORD bytesRead = 0;
    DWORD totalBytesRead = 0;
    char buffer[1024];
    DWORD dwFileSize = 0;
    DWORD dwSize = sizeof(dwFileSize);
    if (HttpQueryInfoA(hUrl, HTTP_QUERY_CONTENT_LENGTH | HTTP_QUERY_FLAG_NUMBER, &dwFileSize, &dwSize, NULL) != TRUE) {
        std::cerr << "Unable to retrieve file size, downloading anyway..." << std::endl;
    }
    else {
        std::cout << "File size obtained from headers: " << dwFileSize << " bytes" << std::endl;
        dwFileSize = dwFileSize;
    }

    while (InternetReadFile(hUrl, buffer, sizeof(buffer), &bytesRead) && bytesRead > 0) {
        outFile.write(buffer, bytesRead);
        totalBytesRead += bytesRead;

        double percentDownloaded = (dwFileSize > 0)
            ? (static_cast<double>(totalBytesRead) / dwFileSize) * 100
            : 0;
    }

    std::cout << "\nDownload complete!" << std::endl;

    outFile.close();
    InternetCloseHandle(hUrl);
    InternetCloseHandle(hInternet);
    return true;
}


static char gelen_key[65] = "";
bool IsKeyValid(const std::string& key, const std::string& keyList) {
    std::istringstream iss(keyList);
    std::string line;
    while (std::getline(iss, line)) {
        if (line == key) {
            return true;
        }
    }
    return false;
}



namespace WebVersion
{

}