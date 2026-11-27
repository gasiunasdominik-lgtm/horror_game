using UnityEngine;
using Vchod.Core;
using Vchod.Player;
using Vchod.Systems;

namespace Vchod.World
{
    /// <summary>A story piece on a wall or floor: a note, a photo or a device that plays something.</summary>
    public sealed class StoryPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject note;
        [SerializeField] private GameObject photo;
        [SerializeField] private GameObject device;

        private int _id;
        private ModuleLayoutBuilder _house;

        public int Id => _id;

        public string Prompt
        {
            get
            {
                var e = StoryCatalog.ById(_id);
                if (e == null) return null;
                return e.Kind switch
                {
                    StoryKind.Photo => "E  Zvednout fotku",
                    StoryKind.Sound => "E  Poslechnout",
                    _ => "E  Přečíst",
                };
            }
        }

        public void Setup(int id, ModuleLayoutBuilder house)
        {
            _id = id;
            _house = house;
            var kind = StoryCatalog.ById(id)?.Kind ?? StoryKind.Note;
            if (note != null) note.SetActive(kind == StoryKind.Note);
            if (photo != null) photo.SetActive(kind == StoryKind.Photo);
            if (device != null) device.SetActive(kind == StoryKind.Sound);
        }

        public void Interact(GameObject interactor)
        {
            var isNew = StoryProgress.Add(_id);
            if (_house != null) _house.MarkStoryTaken(_id);
            EventBus.Publish(new StoryPieceFound { Id = _id, New = isNew });
            Destroy(gameObject);
        }
    }
}
