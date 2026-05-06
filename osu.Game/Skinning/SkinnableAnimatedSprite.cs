// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osuTK;
using LegacySetting = osu.Game.Skinning.SkinConfiguration.LegacySetting;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A skinnable element which uses an animated texture backing.
    /// </summary>
    public partial class SkinnableAnimatedSprite : SkinnableSprite
    {
        [Resolved]
        private ISkinSource source { get; set; } = null!;

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.SpriteName), SettingControlType = typeof(AnimatedSpriteSelectorControl))]
        public override Bindable<string> SpriteName { get; } = new Bindable<string>(string.Empty);

        [SettingSource("Looping", "Whether the animation should be looping")]
        public Bindable<bool> Looping { get; } = new BindableBool(true);

        [SettingSource("Framerate", "Specifies the framerate of the animation affecting its speed")]
        public Bindable<int> Framerate { get; } = new BindableInt
        {
            // TODO find out how to do this less hacky
            MinValue = 0,
            MaxValue = 120,
            Precision = 1
        };

        public SkinnableAnimatedSprite()
            : base(new AnimatedSpriteComponentLookup(string.Empty))
        {
            Looping.BindValueChanged(looping =>
            {
                ((AnimatedSpriteComponentLookup)ComponentLookup).Looping = looping.NewValue;
                if (IsLoaded)
                    SkinChanged(CurrentSkin);
            });

            Framerate.BindValueChanged(framerate =>
            {
                int targetFramerate = framerate.NewValue;
                if (targetFramerate <= 0)
                    targetFramerate = Framerate.Default;

                ((AnimatedSpriteComponentLookup)ComponentLookup).Framerate = targetFramerate;
                if (IsLoaded)
                    SkinChanged(CurrentSkin);
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var framerateSetting = source.GetConfig<LegacySetting, int>(LegacySetting.AnimationFramerate);
            Framerate.Default = framerateSetting?.Value ?? 60;

            if (Framerate.Value == 0)
                Framerate.SetDefault();
        }

        internal class AnimatedSpriteComponentLookup : SpriteComponentLookup
        {
            public bool Looping { get; set; }
            public double Framerate { get; set; }

            public AnimatedSpriteComponentLookup(string textureName, Vector2? maxSize = null, bool looping = true, double framerate = 60)
                : base(textureName, maxSize)
            {
                Looping = looping;
                Framerate = framerate;
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
