using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour {

	// Use this for initialization
	void Start () {
        FindObjectOfType<LuaManager>().StartLoginPanel();
   
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
