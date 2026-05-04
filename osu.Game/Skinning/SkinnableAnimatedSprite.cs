// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osuTK;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A skinnable element which uses an animated texture backing.
    /// </summary>
    public partial class SkinnableAnimatedSprite : SkinnableSprite
    {
        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.SpriteName), SettingControlType = typeof(AnimatedSpriteSelectorControl))]
        public override Bindable<string> SpriteName { get; } = new Bindable<string>(string.Empty);

        [SettingSource("Looping", "Whether the animation should be looping")]
        public Bindable<bool> Looping { get; } = new BindableBool(true);

        public SkinnableAnimatedSprite()
            : base(new AnimatedSpriteComponentLookup(string.Empty))
        {
            Looping.BindValueChanged(looping =>
            {
                ((AnimatedSpriteComponentLookup)ComponentLookup).Looping = looping.NewValue;
                if (IsLoaded)
                    SkinChanged(CurrentSkin);
            });
        }

        internal class AnimatedSpriteComponentLookup : SpriteComponentLookup
        {
            public bool Looping { get; set; }

            public AnimatedSpriteComponentLookup(string textureName, Vector2? maxSize = null, bool looping = true)
                : base(textureName, maxSize)
            {
                Looping = looping;
            }
        }

        public partial class AnimatedSpriteSelectorControl : SpriteSelectorControl
        {
            protected override IEnumerable<string> ApplyFilter(IEnumerable<string> filenames, Skin skin)
            {
                return base.ApplyFilter(filenames, skin)
                           .Select(f => Path.GetFileNameWithoutExtension(f))
                           .Where(f => f.EndsWith("-0", StringComparison.Ordinal))
                           .Distinct()
                           .Select(f => f[..^"-0".Length])
                           .Where(f => skin.IsAnimation(f));
            }
        }
    }
}
