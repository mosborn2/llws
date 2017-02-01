using UnityEngine;
using System.Collections;

public class DebugBox : MonoBehaviour {

  bool state = false;
  public UnityEngine.UI.Text tBox;
	
	void Start () {
    tBox.text = "Press T to show Debug Notes";
	}
	
	//Just add stuff to the string when necessary
	void Update () {
    if (Input.GetKeyUp(KeyCode.T))
    {
      if (state == false)
      {
        tBox.text = "Q - pickup item, E - destroy item (get paid)\n" +
        "I - Show item stats for P1, J - transfer first shop item to P1\n" +
        "P - purchase health bonus for P1, L - spawn pickup cube near middle\n" +
        "Backspace - GODMODE!!, T - shrink this wall of text";
        state = true;
      }
      else
      {
        tBox.text = "Press T to show Debug Notes";
        state = false;
      }
    }

	}
}
