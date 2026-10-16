# Search

void initPlayer()

# add before

#ifdef __AUTO_HUNT__
PyObject* playerGetAutoHuntStatus(PyObject* poSelf, PyObject* poArgs)
{
	return Py_BuildValue("i", CPythonPlayer::Instance().GetAutoHuntStatus());
}
#endif


# Search

		{ NULL,							NULL,								NULL },

# Add before

#ifdef __AUTO_HUNT__
		{ "GetAutoHuntStatus", playerGetAutoHuntStatus, METH_VARARGS },
#endif

