﻿using System;
using System.IO.Abstractions;

namespace PetroGlyph.Games.EawFoc.Services.Detection.Platform
{
    internal class DiskGameValidator : GamePlatformValidator
    {
        public DiskGameValidator(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        
        public override bool IsPlatformFoc(ref IDirectoryInfo location)
        {
            throw new NotImplementedException();
        }

        public override bool IsPlatformEaw(ref IDirectoryInfo location)
        {
            throw new NotImplementedException();
        }
    }
}