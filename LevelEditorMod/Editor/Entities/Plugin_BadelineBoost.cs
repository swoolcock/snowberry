﻿using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace LevelEditorMod.Editor.Entities {
    [Plugin("badelineBoost")]
    public class Plugin_BadelineBoost : Entity {
        [Option("lockCamera")] public bool LockCamera = true;
        [Option("canSkip")] public bool CanSkip = false;
        [Option("finalCh9Boost")] public bool FinalCh9Boost = false;
        [Option("finalCh9GoldenBoost")] public bool FinalCh9GoldenBoost = false;
        [Option("finalCh9Dialog")] public bool FinalCh9Dialog = false;

        public override void Render() {
            base.Render();

            MTexture orb = GFX.Game["objects/badelineboost/idle00"];
            orb.DrawCentered(Position);

            Vector2 prev = Position;
            foreach (Vector2 node in Nodes) {
                orb.DrawCentered(node);
                DrawUtil.DottedLine(prev, node, Color.Red * 0.5f, 8, 4);
                prev = node;
            }
        }
    }
}