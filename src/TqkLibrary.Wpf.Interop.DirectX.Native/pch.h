
#ifndef TQKLIBRARYWPFINTEROPDIRECTXNATIVE_PCH_H
#define TQKLIBRARYWPFINTEROPDIRECTXNATIVE_PCH_H

#ifdef TQKLIBRARYWPFINTEROPDIRECTXNATIVE_EXPORTS
#define TQKLIBRARYWPFINTEROPDIRECTXNATIVEEXPORT extern "C" __declspec( dllexport )
#else
#define TQKLIBRARYWPFINTEROPDIRECTXNATIVEEXPORT extern "C" __declspec( dllimport )
#endif

#define WIN32_LEAN_AND_MEAN// Exclude rarely-used stuff from Windows headers
#include <windows.h>

// C RunTime Header Files
#include <stdlib.h>
#include <stdio.h>

#include "SurfaceQueue.h"

#if DIRECTX_SDK
#include <d3dx9.h>
#else
#include "d3d9.h"
#endif

#include <D3D10_1.h>

#ifdef TQKLIBRARYSCRCPYNATIVE_EXPORTS



#endif // TQKLIBRARYSCRCPYNATIVE_EXPORTS
#include "SurfaceQueueInterop.h"
#include "QueueHelperStruct.h"
#include "Exports.h"

#define IFC(x) { hr = (x); if (FAILED(hr)) { goto Cleanup; }}
#define _ReleaseInterface(x) { if (NULL != x) { x->Release(); x = NULL; }}
#endif // TQKLIBRARYWPFINTEROPDIRECTXNATIVE_PCH_H
