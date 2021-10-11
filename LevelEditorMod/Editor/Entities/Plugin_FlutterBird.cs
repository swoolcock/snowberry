﻿using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [EntityPlugin("flutterbird")]
    public class Plugin_FlutterBird : EntityPlugin {
        private static readonly Color[] colors = new Color[4] {
            Calc.HexToColor("89fbff"),
            Calc.HexToColor("f0fc6c"),
            Calc.HexToColor("f493ff"),
            Calc.HexToColor("93baff"),
        };
        // TODO: per-entity randomness

        public override void Render() {
            base.Render();
            GFX.Game["scenery/flutterbird/idle00"].DrawJustified(Position, new Vector2(0.5f, 1.0f), colors[0]);
        }
    }
}
