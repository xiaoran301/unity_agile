using UnityEngine;

namespace UnityTemplateProjects.Foundation.Base
{
    public class DescriptionBaseSO : SerializableScriptableObject
    {
        [TextArea] public string description;
    }

}