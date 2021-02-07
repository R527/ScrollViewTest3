using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 入力された文字を登録ワードより検索し、禁止ワードの場合には同数の伏字にするクラス
/// </summary>
public class CheckTabooWard : MonoBehaviour {

    public string StrMatch(string src, string[] RegList) {
        string answerWord = "";
        foreach (string checkWord in RegList) {
            int num = src.IndexOf(checkWord);
            if (num >= 0) {
                return answerWord = ChangeWord(src, checkWord);
            }
        }
        return src;
    }



    /// <summary>
    /// 文字変換処理
    /// </summary>
    /// <param name="src"></param>
    /// <param name="regStr"></param>
    /// <returns></returns>
    private string ChangeWord(string src, string regStr) {
        string turnoffWord = "";
        for (int i = 0; i < regStr.Length; i++) {
            turnoffWord += "*";
        }
        string newWord = src.Replace(regStr, turnoffWord);
        return newWord;
    }
}