using UnityEngine;
using System.Collections;

public class addp : MonoBehaviour {
    int[] a = { 0, 0, 0 };
	// Use this for initialization
	void Start () {
        //int a = addp1(1, 1,30);
        //int a = GetNumber(30);
        //print(a);
       // search(0);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public  int addp1(int a, int b, int index)
    {     
        if (index<3)
        {
            return b;
        }
        else {
            index--;
            return addp1(b, a + b, index);
        }         
    }

    public int GetNumber(int index) {
       int   result = 1;
        if (index > 2) {

            result = GetNumber(index - 1) + GetNumber(index - 2);
        }
        return result;
    }
    public void search(int index) {
        if (index > 2) {
            if (a[0] * 100 + a[1] * 10 + a[2] + a[0] + a[1] * 10 + a[2] * 100 == 1333) {
                print(a[0] + ","+a[1] + ","+a[2]);
            }
            return;
        }
        for (int i = 0; i < 10; i++)
        {
            a[index] = i;
            search(index + 1);
        }
        return;

    }

}
