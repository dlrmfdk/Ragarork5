using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCardEffect : MonoBehaviour
{
  public abstract void Execute(Player player ,Enemy target = null);

 
}
