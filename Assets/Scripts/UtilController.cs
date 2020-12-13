using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UtilController : MonoBehaviour
{
    private VariablesController variablesController;
    private AudioSource outAudioSource;
    private Queue<string> outAudioPlayList;
    private Animator characterAnimator;

    void Start()
    {
        variablesController = GameObject.Find("recordButton").GetComponent<VariablesController>();
        outAudioSource = GameObject.Find("outAudioSource").GetComponent<AudioSource>();
        characterAnimator = GameObject.Find("character").GetComponent<Animator>();

        outAudioPlayList = new Queue<string>();
    }

    [System.Obsolete]
    void Update()
    {
        // 화면 메시징 관련 Update
        // 개발 여부가 활성화 되어 있는 경우, 디버깅 메시지를 화면에 뿌려준다.
        if (variablesController.isBoolDevYn())
        {
            GameObject.Find("devLog1").GetComponent<Text>().text = variablesController.getStrLogText1();
            GameObject.Find("devLog2").GetComponent<Text>().text = variablesController.getStrLogText2();
            GameObject.Find("devLog3").GetComponent<Text>().text = variablesController.getStrLogText3();
            GameObject.Find("devErr").GetComponent<Text>().text = variablesController.getStrErrText();
        }

        // Trigger 관련 Update
        // 트리거 값이 있을 경우, 값을 처리한다.
        if ("-1".Equals(variablesController.getStrIntendTrigger()) == false)
        {
            if ("Trigger".Equals(variablesController.getStrIntendTrigger()))
            {
                variablesController.setStrIntendTrigger("-1");
            } else
            {
                Debug.Log(variablesController.getStrIntendTrigger());
                characterAnimator.SetTrigger(variablesController.getStrIntendTrigger());
                variablesController.setStrIntendTrigger("-1");
            }
        }

        // 오디오 관련 Update
        outAudioPlayList = variablesController.getQuePlayFiles();

        try
        {
            if (outAudioPlayList.Count > 0)
            {
                string ttsFile = outAudioPlayList.Dequeue();

                WWW www = new WWW("File://" + ttsFile);
                while (!www.isDone)
                {
                }

                // 현재 MP3는 모바일에 한해 License가 발생하지 않아 play 하는데 문제가 없는데, build target이 모바일이 아닐 경우 라이센스 문제 발생할 수 있음.
                AudioClip clip = www.GetAudioClip(false);
                clip.name = Path.GetFileName(ttsFile);
                // 지금 이야기 중이 아니면, 이야기 함. clip을 배열로 넣어서 Queue 형태로 처리 할 수 있긴 한데, 이 경우 CallBack 순서가 비정상적으로 처리되므로 문제가 될 수 있음. 일단은 이야기 중인 경우, 이야기 씹도록 처리.
                if (variablesController.isBooOutAudioPlayingYn() == false)
                {
                    outAudioSource.PlayOneShot(clip);
                }

                // queue형태이므로, 사실 아래 초기화 하지 않으면, 파일 온 것들 다 재생 시켜주는데, 현재 재생건만 재생하고 나머지는 버려버리게 일단 구현함
                // 추후 음성 재생 구현 방식을 다시 고려해볼 때 아래 부분 없애는 것도 생각해보면 좋을 듯.
                variablesController.setQuePlayFiles(new Queue<string>());
            }
        } catch (Exception e)
        {
            setLog(e.ToString());
            variablesController.setQuePlayFiles(new Queue<string>());
        }
    }

    // 개발 셋팅값에 따라 화면 로그창 Show/Hide 처리
    public void initDevLog()
    {
        GameObject.Find("devLog1").SetActive(variablesController.isBoolDevYn());
        GameObject.Find("devLog2").SetActive(variablesController.isBoolDevYn());
        GameObject.Find("devLog3").SetActive(variablesController.isBoolDevYn());
        GameObject.Find("devErr").SetActive(variablesController.isBoolDevYn());
    }

    // 개발 셋팅값에 따라 화면 로그창 Show/Hide 처리
    public void initRecord()
    {
        variablesController.setBooPressYn(true);
        variablesController.setBooRecordYn(true);
        variablesController.setStrLogText1("");
        variablesController.setStrLogText2("");
        variablesController.setStrLogText3("");
        variablesController.setStrErrText("");
    }

    // 기본으로 Console에는 로그가 찍히고, 개발 플레그가 true일 경우, 화면 로그창에 값도 셋팅해줌.
    public void setLog(string strObj, string msg ) {
        Debug.Log(strObj + " : " + msg);

        if (variablesController.isBoolDevYn() == true)
        {
            if ("devLog1".Equals(strObj))
            {
                variablesController.setStrLogText1(msg);
            } else if ("devLog2".Equals(strObj))
            {
                variablesController.setStrLogText2(msg);
            }
            else if ("devLog3".Equals(strObj))
            {
                variablesController.setStrLogText3(msg);
            }
            else
            {
                variablesController.setStrErrText(msg);
            }
        }
    }

    // 메서드의 생성 및 이용 규칙은 javascript처럼 느슨하지 않고, Java 같은 느낌임.
    // 파라미터를 1개만 줄 경우, Err로그로 남게 처리함. Debug.Err 이런 것도 있을 것 같긴 한데, 화면이랑 콘솔에 간단한 메시지 찍는거라서 따로 안함.
    public void setLog(string msg)
    {
        setLog("devErr", msg);
    }

    // 입력 스피커가 미사용 중일 경우 true 반환
    public bool isInAudioStop()
    {
        if (variablesController.isBooInAudioPlayingYn() == false)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // 출력 스피커가 미사용 중일 경우 true 반환
    public bool isOutAudioStop()
    {
        if (variablesController.isBooOutAudioPlayingYn() == false)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    // 입력 스피커와 출력 스피커가 모두 미사용 중일 경우 true 반환
    public bool isAllAudioStop()
    {
        if (variablesController.isBooInAudioPlayingYn() == false && variablesController.isBooOutAudioPlayingYn() == false)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /* playForceYn은 현재 출력중인 음성 메시지가 있을 시, 강제로 중지하고 에러 메시지를 출력하냐 안 하냐의 차이
     *   강제여부 / 아웃Audio 현재 음성 재생 여부
        1. true / true -> stop / play
        2. true / false -> play
        3. false / true - return
        4. false / false - play  */
    public void playErrorAudio(string errAudio, bool playForceYn) 
    {
        if (variablesController.isBooOutAudioPlayingYn())
        {
            if (playForceYn)
            {
                outAudioSource.Stop();
            } else
            {
                return;
            }
        }

        outAudioSource.clip = (AudioClip)Resources.Load(errAudio);
        outAudioSource.Play();
    }

    // 날짜 포멧에 맞춰서 값을 반환
    public string getCurrentTimeFormat(string format)
    {
        return System.DateTime.Now.ToString(format);
    }
}
 