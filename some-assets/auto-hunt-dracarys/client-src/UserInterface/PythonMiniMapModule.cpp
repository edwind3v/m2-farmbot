# Search

void initMiniMap()

# Add before

#ifdef __AUTO_HUNT__
PyObject* miniMapSetAutoHuntRangeStatus(PyObject* poSelf, PyObject* poArgs)
{
	bool bStatus;
	if (!PyTuple_GetBoolean(poArgs, 0, &bStatus))
		return Py_BuildException();
	CPythonMiniMap::Instance().SetAutoHuntRange(bStatus);
	return Py_BuildNone();
}
PyObject* miniMapSetAutoHuntRange(PyObject* poSelf, PyObject* poArgs)
{
	float fVal;
	if (!PyTuple_GetFloat(poArgs, 0, &fVal))
		return Py_BuildException();
	CPythonMiniMap::Instance().ChangeAutoHuntRange(fVal);
	return Py_BuildNone();
}
#endif

# Search

		{ NULL, NULL },

# Add before

#ifdef __AUTO_HUNT__
		{ "SetAutoHuntRangeStatus",						miniMapSetAutoHuntRangeStatus,								METH_VARARGS },
		{ "SetAutoHuntRange",						miniMapSetAutoHuntRange,								METH_VARARGS },
#endif


