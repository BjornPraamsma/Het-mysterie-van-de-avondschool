using System.Collections.Generic;

namespace Game.Models
{
    public enum Direction { Front, Back, Left, Right }

    public class CharacterSpriteConfig
    {
        public Dictionary<Direction, List<string>> IdleFrames { get; set; } = new();
        public Dictionary<Direction, List<string>> WalkFrames { get; set; } = new();

        public static CharacterSpriteConfig LoadBlondeMan()
        {
            string baseFolder = "blonde_man/";

            return new CharacterSpriteConfig
            {
                IdleFrames = new Dictionary<Direction, List<string>>
                {
                    { Direction.Front, new() { baseFolder + "IdleFrontTop1BlondeMan.png", baseFolder + "IdleFrontTop2BlondeMan.png" } },
                    { Direction.Back,  new() { baseFolder + "IdleBackTopBlondMan.png", baseFolder + "IdleBackTop1BlondMan.png" } },
                    { Direction.Left,  new() { baseFolder + "IdleLeftTop1BlondMan.png", baseFolder + "IdleLeftTop2BlondMan.png" } },
                    { Direction.Right, new() { baseFolder + "IdleRightTop1BlondMan.png", baseFolder + "IdleRightTop2BlondMan.png" } }
                },
                WalkFrames = new Dictionary<Direction, List<string>>
                {
                    { Direction.Front, new() { baseFolder + "WalkFrontStrideLeftBlondMan.png", baseFolder + "WalkFrontStand1BlondMan.png", baseFolder + "WalkFrontStrideRightBlondMan.png" } },
                    { Direction.Back,  new() { baseFolder + "WalkBackStrideLeftBlondMan.png", baseFolder + "WalkBackStand1BlondMan.png", baseFolder + "WalkBackStrideRightBlondMan.png" } },
                    { Direction.Left,  new() { baseFolder + "WalkLeftStrideLeftBlondMan.png", baseFolder + "WalkLeftStand1BlondMan.png", baseFolder + "WalkLeftStrideRightBlondMan.png" } },
                    { Direction.Right, new() { baseFolder + "WalkRightStrideLeftBlondMan.png", baseFolder + "WalkRightStand1BlondMan.png", baseFolder + "WalkRightStrideRightBlondMan.png" } }
                }
            };
        }
    }
}