using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 入力された文字を登録ワードより検索し、禁止ワードの場合には同数の伏字にするクラス
/// </summary>
public class CheckTabooWard : MonoBehaviour {

    //main
    //public List<string> tabooList;
    //public string word;

    /// <summary>
    /// Debug用
    /// </summary>
    /// <param name="src"></param>
    /// <param name="RegList"></param>
    /// <returns></returns>
    //private void Start() {
    //    Debug.Log(word);
    //    string choice = StrMatch(word,tabooList);
    //    Debug.Log(choice);
    //}

    public string StrMatch(string src, string[] RegList) {
        string answerWord = "";
        foreach (string checkWord in RegList) {
            int num = src.IndexOf(checkWord);
            if (num >= 0) {
                return answerWord = ChangeWord(src, checkWord);
            }
        }
        return answerWord = src;
        //Debug.Log(src);
        //string checkword = "";
        //foreach (string RegStr in RegList) {
        //    Debug.Log(RegStr);
        //    if ((RegStr.StartsWith("%")) && (RegStr.EndsWith("%"))) {
        //        // 部分一致
        //        if (src.Contains((RegStr.Remove(0, 1)).Remove(RegStr.Length - 2))) {
        //            Debug.Log(RegStr);
        //            Debug.Log(src);
        //            return checkword = ChangeWord(src, RegStr);
        //        }
        //    } else if (RegStr.StartsWith("%")) {
        //        // 後方一致
        //        if (src.EndsWith(RegStr.Remove(0, 1))) {
        //            Debug.Log(RegStr);
        //            Debug.Log(src);
        //            return checkword = ChangeWord(src, RegStr);
        //        }
        //    } else if (RegStr.EndsWith("%")) {
        //        // 前方一致
        //        if (src.StartsWith(RegStr.Remove(RegStr.Length - 1))) {
        //            Debug.Log(RegStr);
        //            Debug.Log(src);
        //            return checkword = ChangeWord(src, RegStr);
        //        }
        //    } else {
        //        // 完全一致
        //        if (src == RegStr) {
        //            Debug.Log(RegStr);
        //            Debug.Log(src);
        //            return src;
        //        }
        //    }
        //}
        //Debug.Log(checkword);
        //Debug.Log("false");
        //return checkword = src;
    }



    /// <summary>
    /// 文字変換処理
    /// </summary>
    /// <param name="src"></param>
    /// <param name="regStr"></param>
    /// <returns></returns>
    private string ChangeWord(string src, string regStr) {
        //string change = regStr.Remove(0, 1).Remove(regStr.Length - 2);
        string turnoffWord = "";
        for (int i = 0; i < regStr.Length; i++) {
            turnoffWord += "*";
        }
        Debug.Log(turnoffWord);
        Debug.Log(regStr);
        string newWord = src.Replace(regStr, turnoffWord);
        return newWord;
    }
}