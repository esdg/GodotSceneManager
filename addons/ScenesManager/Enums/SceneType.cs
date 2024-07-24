namespace MoF.Addons.ScenesManager.Enums
{
    /// <summary>
    /// Specifies the type of the target scene in the scenes manager.
    /// </summary>
    public enum TargetSceneType
    {
        /// <summary>
        /// Represents a node that starts the application.
        /// </summary>
        StartAppGraphNode,

        /// <summary>
        /// Represents a node that is part of the scene graph.
        /// </summary>
        SceneGraphNode,

        /// <summary>
        /// Represents a node that quits the application.
        /// </summary>
        QuitGraphNode,
    }
}
