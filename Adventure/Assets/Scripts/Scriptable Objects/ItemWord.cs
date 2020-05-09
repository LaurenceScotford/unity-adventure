using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/Commands/Item")]
public class ItemWord : Command
{

    [SerializeField] private string associatedItem = null;

    public string AssociatedItem { get { return associatedItem; } }
}
    