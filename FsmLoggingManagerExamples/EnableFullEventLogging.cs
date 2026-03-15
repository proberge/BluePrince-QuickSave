var selectedGameObject = Paste();

GameObject gameObject = (GameObject)selectedGameObject;

PlayMakerFSM fsm = gameObject.GetComponent<PlayMakerFSM>();
if (fsm == null)
{
    Log($"Could not find PlayMakerFSM on '{gameObject.name}'");
    return;
}

fsm.enableFullFsmLogging();
