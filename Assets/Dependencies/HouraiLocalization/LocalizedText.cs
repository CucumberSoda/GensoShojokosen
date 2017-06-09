using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HouraiTeahouse.Localization {

    /// <summary> An AbstractLocalizedText where the localization key is defined via serializaiton </summary>
    [HelpURL("http://wiki.houraiteahouse.net/index.php/Dev:Localization#Localized_Text")]
    public sealed class LocalizedText : AbstractLocalizedText {

        /// <summary> The format for the localization string to be displayed in. </summary>
        /// <see cref="string.Format" />
        [SerializeField]
        string _format;

        /// <summary> The serialized localization key </summary>
        [SerializeField]
        string _key;

        /// <summary> Gets or sets the localization key of the LocalizedText </summary>
        public string Key {
            get { return NativeText; }
            set { NativeText = value; }
        }

        /// <summary> Unity callback. Called once before the object's first frame. </summary>
        protected override void Awake() {
            base.Awake();
            if (HasComponent() && string.IsNullOrEmpty(_key))
                _key = GetText();
            NativeText = _key;
        }

        /// <summary>
        ///     <see cref="AbstractLocalizedText" />
        /// </summary>
        protected override string Process(string val) { return _format.IsNullOrEmpty() ? val : _format.With(val); }

    }

}
