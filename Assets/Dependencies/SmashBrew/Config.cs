﻿using UnityEngine;

namespace Hourai.SmashBrew {
    
    [CreateAssetMenu(fileName = "New Config", menuName = "SmashBrew/Config")]
    public class Config : ScriptableObject {

        #region Serialized Fields

        [SerializeField]
        private Color[] PlayerColors = { Color.red, Color.blue, Color.green, Color.yellow };

        [SerializeField]
        private Color _cpuColor = Color.grey;

        [SerializeField]
        private Color DamageableHitboxColor = Color.yellow;

        [SerializeField]
        private Color IntangibleHitboxColor = Color.blue;

        [SerializeField]
        private Color InvincibleHitboxColor = Color.green;
        
        [SerializeField]
        private Color OffensiveHitboxColor = Color.red;

        public Color CPUColor {
            get { return _cpuColor; }
        }

        #endregion

        private static Config _instance;

        public static Config Instance {
            get {
                if(_instance)
                    return _instance;
                var configs = Resources.LoadAll<Config>(string.Empty);
                if (configs.Length > 0)
                    return _instance = configs[0];
                return _instance = CreateInstance<Config>();
            }
        }

        /// <summary>
        /// The maximum number of players supported in one match
        /// 
        /// </summary>
        public int MaxPlayers {
            get { return Instance.PlayerColors.Length; }
        }
        
        public Color GetPlayerColor(int playerNumber) {
            return playerNumber < 0 || playerNumber >= MaxPlayers
                       ? Color.white
                       : Instance.PlayerColors[playerNumber];
        }

        public Color GetHitboxColor(Hitbox.Type type) {
            switch (type) {
                case Hitbox.Type.Offensive:
                    return Instance.OffensiveHitboxColor;
                case Hitbox.Type.Damageable:
                    return Instance.DamageableHitboxColor;
                case Hitbox.Type.Invincible:
                    return Instance.IntangibleHitboxColor;
                case Hitbox.Type.Intangible:
                    return Instance.InvincibleHitboxColor;
                default:
                    return Color.magenta;
            }
        }
        
    }

}