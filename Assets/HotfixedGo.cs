using UnityEngine;
using System.Collections;
using XLua;

[Hotfix]
public class HotfixedGo : MonoBehaviour {
    private string a = "1111111111111111111111111";
    private string b = "2222222222222222222222222";
    // Use this for initialization
         IEnumerator Start () {

        while (true)
        {
            yield return new WaitForSeconds(3);
            HotFixFunc1();
            yield return new WaitForSeconds(3);
            HotFixFunc2();
        }
    }
	
	// Update is called once per frame
	void Update () {
       
	}

    void  HotFixFunc1() {
        Debug.Log(a);
    }
    void HotFixFunc2()
    {
        Debug.Log(b);
    }
}
