﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Windows.Win32
{
    internal static partial class PInvoke
    {
        public static BOOL IsChild<T>(HWND hWndParent, T hWnd) where T : IHandle<HWND>
        {
            BOOL result = IsChild(hWndParent, hWnd.Handle);
            GC.KeepAlive(hWnd.Wrapper);
            return result;
        }

        public static BOOL IsChild<T>(in T hWndParent, HWND hWnd) where T : IHandle<HWND>
        {
            BOOL result = IsChild(hWndParent.Handle, hWnd);
            GC.KeepAlive(hWndParent.Wrapper);
            return result;
        }

        public static BOOL IsChild<T>(in T hWndParent, T hWnd) where T : IHandle<HWND>
        {
            BOOL result = IsChild(hWndParent.Handle, hWnd.Handle);
            GC.KeepAlive(hWndParent.Wrapper);
            GC.KeepAlive(hWnd.Wrapper);
            return result;
        }
    }
}
