#pragma once
#include <windows.h>
#include <string>
#include <stdexcept>
#include <fstream>
#include <sstream>
#include <iostream>

class Exception : public std::runtime_error {
public:
    explicit Exception(const std::string& msg) : std::runtime_error(msg) {}
};

class python27 {
public:
    explicit python27(HMODULE pythonModule) {
        loader(pythonModule);
        if (p_PyEval_InitThreads) p_PyEval_InitThreads();
    }

    // EKLENEN FONKSİYON - RunSimpleString
    void RunSimpleString(const std::string& code) {
        if (!p_PyRun_SimpleStringFlags) {
            throw Exception("PyRun_SimpleStringFlags not loaded");
        }
        void* g = AcquireGIL();
        int result = p_PyRun_SimpleStringFlags(code.c_str(), nullptr);
        ReleaseGIL(g);
        if (result != 0) {
            throw Exception("RunSimpleString failed");
        }
    }

    void* Function(const char* Module, const char* function) {
        void* g = AcquireGIL();
        void* mod = p_PyImport_ImportModule(Module);
        if (!mod) {
            ReleaseGIL(g);
            throw Exception(std::string("Failed to import module ") + Module);
        }
        void* func = p_PyObject_GetAttrString(mod, function);
        if (!func) {
            p_Py_DecRef(mod);
            ReleaseGIL(g);
            throw Exception(std::string("Failed to get func ") + function);
        }
        void* result = p_PyObject_CallObject(func, nullptr);
        p_Py_DecRef(func);
        p_Py_DecRef(mod);
        ReleaseGIL(g);
        if (!result)
            throw Exception(std::string("Call failed ") + function);
        return result;
    }

    void* WithTuple(void* Obj, void* Argument) {
        if (!Obj || !Argument)
            throw Exception("WithTuple: Obj or args is null");
        void* g = AcquireGIL();
        void* result = p_PyObject_CallObject(Obj, Argument);
        ReleaseGIL(g);
        if (!result)
            throw Exception("WithTuple failed");
        return result;
    }

    void* AcquireGIL() {
        if (p_PyGILState_Ensure)
            return p_PyGILState_Ensure();
        return nullptr;
    }

    void ReleaseGIL(void* state) {
        if (p_PyGILState_Release && state)
            p_PyGILState_Release(state);
    }

    int TupleSize(void* obj) {
        return p_PyTuple_Size ? (int)p_PyTuple_Size(obj) : 0;
    }

    void* TupleGetItem(void* obj, int idx) {
        return p_PyTuple_GetItem ? p_PyTuple_GetItem(obj, idx) : nullptr;
    }

    double FloatAsDouble(void* obj) {
        return p_PyFloat_AsDouble ? p_PyFloat_AsDouble(obj) : 0.0;
    }

    long LongAsLong(void* obj) {
        return p_PyLong_AsLong ? p_PyLong_AsLong(obj) : 0;
    }

    void DecRef(void* obj) {
        if (p_Py_DecRef && obj)
            p_Py_DecRef(obj);
    }

    void* Tuple_New(long size) {
        if (!p_PyTuple_New)
            throw Exception("PyTuple_New not loaded");
        return p_PyTuple_New(size);
    }

    int Tuple_SetItem(void* tuple, long idx, void* item) {
        if (!p_PyTuple_SetItem)
            throw Exception("PyTuple_SetItem not loaded");
        return p_PyTuple_SetItem(tuple, idx, item);
    }

    void* Long_FromLong(long value) {
        if (!p_PyLong_FromLong)
            throw Exception("PyLong_FromLong not loaded");
        return p_PyLong_FromLong(value);
    }

    void* Float_FromDouble(double value) {
        if (!p_PyFloat_FromDouble)
            throw Exception("PyFloat_FromDouble not loaded");
        return p_PyFloat_FromDouble(value);
    }

    void* String_FromString(const char* str) {
        if (!p_PyString_FromString)
            throw Exception("PyString_FromString not loaded");
        return p_PyString_FromString(str);
    }

    // Public members
    void* (*p_PyImport_ImportModule)(const char*) = nullptr;
    void* (*p_PyObject_GetAttrString)(void*, const char*) = nullptr;

private:
    int (*p_PyRun_SimpleStringFlags)(const char*, void*) = nullptr;
    void* (*p_PyGILState_Ensure)() = nullptr;
    void (*p_PyGILState_Release)(void*) = nullptr;
    void (*p_PyEval_InitThreads)() = nullptr;
    void* (*p_PyObject_CallObject)(void*, void*) = nullptr;
    long (*p_PyLong_AsLong)(void*) = nullptr;
    void (*p_Py_DecRef)(void*) = nullptr;
    long (*p_PyTuple_Size)(void*) = nullptr;
    void* (*p_PyTuple_GetItem)(void*, int) = nullptr;
    double (*p_PyFloat_AsDouble)(void*) = nullptr;
    void* (*p_PyTuple_New)(long) = nullptr;
    int (*p_PyTuple_SetItem)(void*, long, void*) = nullptr;
    void* (*p_PyLong_FromLong)(long) = nullptr;
    void* (*p_PyFloat_FromDouble)(double) = nullptr;
    void* (*p_PyString_FromString)(const char*) = nullptr;

    void loader(HMODULE pythonModule) {
        if (!pythonModule)
            throw Exception("module is null");

        p_PyRun_SimpleStringFlags = reinterpret_cast<decltype(p_PyRun_SimpleStringFlags)>(
            GetProcAddress(pythonModule, "PyRun_SimpleStringFlags"));
        p_PyGILState_Ensure = reinterpret_cast<decltype(p_PyGILState_Ensure)>(
            GetProcAddress(pythonModule, "PyGILState_Ensure"));
        p_PyGILState_Release = reinterpret_cast<decltype(p_PyGILState_Release)>(
            GetProcAddress(pythonModule, "PyGILState_Release"));
        p_PyEval_InitThreads = reinterpret_cast<decltype(p_PyEval_InitThreads)>(
            GetProcAddress(pythonModule, "PyEval_InitThreads"));
        p_PyImport_ImportModule = reinterpret_cast<decltype(p_PyImport_ImportModule)>(
            GetProcAddress(pythonModule, "PyImport_ImportModule"));
        p_PyObject_GetAttrString = reinterpret_cast<decltype(p_PyObject_GetAttrString)>(
            GetProcAddress(pythonModule, "PyObject_GetAttrString"));
        p_PyObject_CallObject = reinterpret_cast<decltype(p_PyObject_CallObject)>(
            GetProcAddress(pythonModule, "PyObject_CallObject"));
        p_PyLong_AsLong = reinterpret_cast<decltype(p_PyLong_AsLong)>(
            GetProcAddress(pythonModule, "PyLong_AsLong"));
        p_Py_DecRef = reinterpret_cast<decltype(p_Py_DecRef)>(
            GetProcAddress(pythonModule, "Py_DecRef"));
        p_PyTuple_Size = reinterpret_cast<decltype(p_PyTuple_Size)>(
            GetProcAddress(pythonModule, "PyTuple_Size"));
        p_PyTuple_GetItem = reinterpret_cast<decltype(p_PyTuple_GetItem)>(
            GetProcAddress(pythonModule, "PyTuple_GetItem"));
        p_PyFloat_AsDouble = reinterpret_cast<decltype(p_PyFloat_AsDouble)>(
            GetProcAddress(pythonModule, "PyFloat_AsDouble"));
        p_PyTuple_New = reinterpret_cast<decltype(p_PyTuple_New)>(
            GetProcAddress(pythonModule, "PyTuple_New"));
        p_PyTuple_SetItem = reinterpret_cast<decltype(p_PyTuple_SetItem)>(
            GetProcAddress(pythonModule, "PyTuple_SetItem"));
        p_PyLong_FromLong = reinterpret_cast<decltype(p_PyLong_FromLong)>(
            GetProcAddress(pythonModule, "PyLong_FromLong"));
        p_PyFloat_FromDouble = reinterpret_cast<decltype(p_PyFloat_FromDouble)>(
            GetProcAddress(pythonModule, "PyFloat_FromDouble"));
        p_PyString_FromString = reinterpret_cast<decltype(p_PyString_FromString)>(
            GetProcAddress(pythonModule, "PyString_FromString"));

        if (!p_PyGILState_Ensure || !p_PyGILState_Release || !p_PyImport_ImportModule ||
            !p_PyObject_GetAttrString || !p_PyObject_CallObject || !p_Py_DecRef ||
            !p_PyTuple_Size || !p_PyTuple_GetItem || !p_PyFloat_AsDouble ||
            !p_PyTuple_New || !p_PyTuple_SetItem || !p_PyLong_FromLong) {
            throw Exception("Critical Python functions not loaded!");
        }
    }
};

inline bool pyscript_load(const std::string& filename, python27& python) {
    std::ifstream file(filename);
    if (!file.is_open()) {
        std::cout << "[ERROR] File not found: " << filename << std::endl;
        return false;
    }
    std::stringstream buffer;
    buffer << file.rdbuf();
    std::string code = buffer.str();
    try {
        python.RunSimpleString(code);
        return true;
    }
    catch (const Exception& e) {
        std::cout << "[ERROR] Script load failed: " << e.what() << std::endl;
        return false;
    }
}

namespace engsdk {
    inline double CallFloatFunction(python27& py, const char* module, const char* func) {
        void* result = nullptr;
        try {
            result = py.Function(module, func);
        }
        catch (const Exception& e) {
            std::cout << "[ERROR] " << e.what() << std::endl;
            return 0.0;
        }
        if (!result) return 0.0;

        void* gil = py.AcquireGIL();
        double value = py.FloatAsDouble(result);
        py.ReleaseGIL(gil);
        py.DecRef(result);
        return value;
    }

    inline long CallIntFunction(python27& py, const char* module, const char* func) {
        void* result = nullptr;
        try {
            result = py.Function(module, func);
        }
        catch (const Exception& e) {
            std::cout << "[ERROR] " << e.what() << std::endl;
            return 0;
        }
        if (!result) return 0;

        void* gil = py.AcquireGIL();
        long value = py.LongAsLong(result);
        py.ReleaseGIL(gil);
        py.DecRef(result);
        return value;
    }

    inline void CallFunction1Int(python27& py, const char* module, const char* func, long arg1) {
        try {
            void* gil = py.AcquireGIL();
            void* mod = py.p_PyImport_ImportModule(module);
            if (!mod) {
                py.ReleaseGIL(gil);
                return;
            }

            void* funcObj = py.p_PyObject_GetAttrString(mod, func);
            if (!funcObj) {
                py.DecRef(mod);
                py.ReleaseGIL(gil);
                return;
            }

            void* args = py.Tuple_New(1);
            py.Tuple_SetItem(args, 0, py.Long_FromLong(arg1));

            void* result = py.WithTuple(funcObj, args);

            py.DecRef(result);
            py.DecRef(args);
            py.DecRef(funcObj);
            py.DecRef(mod);
            py.ReleaseGIL(gil);
        }
        catch (const Exception& e) {
            std::cout << "[ERROR] " << e.what() << std::endl;
        }
    }

    inline void CallFunction2Int(python27& py, const char* module, const char* func,
        long arg1, long arg2) {
        try {
            void* gil = py.AcquireGIL();
            void* mod = py.p_PyImport_ImportModule(module);
            if (!mod) {
                py.ReleaseGIL(gil);
                return;
            }

            void* funcObj = py.p_PyObject_GetAttrString(mod, func);
            if (!funcObj) {
                py.DecRef(mod);
                py.ReleaseGIL(gil);
                return;
            }

            void* args = py.Tuple_New(2);
            py.Tuple_SetItem(args, 0, py.Long_FromLong(arg1));
            py.Tuple_SetItem(args, 1, py.Long_FromLong(arg2));

            void* result = py.WithTuple(funcObj, args);

            py.DecRef(result);
            py.DecRef(args);
            py.DecRef(funcObj);
            py.DecRef(mod);
            py.ReleaseGIL(gil);
        }
        catch (const Exception& e) {
            std::cout << "[ERROR] " << e.what() << std::endl;
        }
    }
}