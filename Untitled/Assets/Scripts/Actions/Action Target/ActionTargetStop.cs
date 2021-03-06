using HutongGames.PlayMaker;

namespace Game.Actions {
    [ActionCategory("Game")]
    [Tooltip("Stops the action, this will clear out any listeners currently set on the target")]
    public class ActionTargetStop : M8.PlayMaker.FSMActionComponentBase<ActionTarget> {

        // Code that runs on entering the state.
        public override void OnEnter() {
            base.OnEnter();

            if(mComp != null)
                mComp.StopAction();

            Finish();
        }
    }
}
