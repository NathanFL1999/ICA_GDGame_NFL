using GDLibrary.Actors;
using GDLibrary.Controllers;
using GDLibrary.Enums;
using GDLibrary.Events;

namespace GDGame.MyGame.Controllers
{
    public class DeathCountController : Controller
    {
        private UITextObject deathCount;

        public DeathCountController(string id, ControllerType controllerType, UITextObject deathCount) : base(id, controllerType)
        {
            this.deathCount = deathCount;
            EventDispatcher.Subscribe(EventCategoryType.UI, HandleEvents);
        }

        private void HandleEvents(EventData eventData)
        {
            if (eventData.EventActionType == EventActionType.OnDeathCountChange)
            {
                deathCount.Text = "Death Count: " + ((int)eventData.Parameters[0]);
            }
        }
    }
}
