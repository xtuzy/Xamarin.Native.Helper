﻿using Yang.Maui.Helper.CustomControls.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Yang.Maui.Helper.CustomControls.DrawableView
{
    public interface IDrawableView : IView
    {
        event EventHandler<PlatformDrawEventArgs> PaintSurface;
        event EventHandler Loaded;
        event EventHandler Unloaded;

        event EventHandler TouchDown;
        event EventHandler TouchMove;
        event EventHandler TouchUp;

        void Invalidate();

        void Load();
        void Unload();

        void OnTouchDown(Point point);
        void OnTouchMove(Point point);
        void OnTouchUp(Point point);

        void OnDraw(object? sender, PlatformDrawEventArgs e);
    }
}
