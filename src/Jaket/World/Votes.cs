namespace Jaket.World;

using System.Collections.Generic;
using UnityEngine.UI;

using Jaket.Assets;
using Jaket.Content;
using Jaket.Net;
using Jaket.UI.Elements;

/// <summary> Class that manages voting for the skip of a cutscene or an option at 2-S. </summary>
public class Votes
{
    /// <summary> Voted players' ids and their votes. </summary>
    public static Dictionary<uint, byte> Ids2Votes = new();
    /// <summary> Current voting taking all updates. </summary>
    public static Voting CurrentVoting;

    /// <summary> Loads the vote system. </summary>
    public static void Load()
    {
        Events.OnLoad += () =>
        {
            if (LobbyController.Online) Init();
        };
        Events.OnLobbyEnter += Init;
    }

    /// <summary> Initializes the vote system. </summary>
    public static void Init()
    {
        if (Scene == "Level 2-S") Init2S();
        ResFind<CutsceneSkip>().Each(IsReal, cs => cs.gameObject.AddComponent<Voting>());
    }

    /// <summary> Votes for the given option. </summary>
    public static void Vote(byte option = 0) => Networking.Send(PacketType.Vote, 5, w =>
    {
        w.Id(AccId);
        w.Byte(option);
    });

    /// <summary> Updates the vote of the given player. </summary>
    public static void UpdateVote(uint owner, byte vote)
    {
        Ids2Votes[owner] = vote;
        CurrentVoting?.Invoke("UpdateVotes", 0f);
    }

    /// <summary> Counts the amount of votes for the given option. </summary>
    public static int Count(byte vote) => Ids2Votes.Count(p => p.Value == vote);

    #region 2-S

    /// <summary> Replaces Mirage with Vermicelli, patches buttons and other minor stuff. </summary>
    public static void Init2S()
    {
        var fallen = ObjFind("Canvas/PowerUpVignette/Panel/Aspect Ratio Mask/Fallen");
        for (int i = 0; i < 4; i++)
            fallen.transform.GetChild(i).GetComponent<Image>().sprite = ModAssets.ChanFallen;

        ResFind<SpritePoses>().Each(sp => IsReal(sp) && sp.copyChangeTo.Length > 0, sp => sp.poses = ModAssets.ChanPoses);

        ObjFind("Canvas/PowerUpVignette/Panel/Intro/Text").AddComponent<Voting>();
        ObjFind("Canvas/PowerUpVignette/Panel/Intro/Text (1)").AddComponent<Voting>();

        ObjFind("Canvas/PowerUpVignette/Panel/Aspect Ratio Mask").transform.Each(act =>
        {
            act.Each(dialog =>
            {
                if (dialog.name.Contains("Dialogue"))
                    dialog.transform.GetChild(0).gameObject.AddComponent<Voting>();

                if (dialog.name.Contains("Choices"))
                    dialog.gameObject.AddComponent<Voting>();
            });
        });

        var fix = ObjFind("Canvas/PowerUpVignette/Panel/Aspect Ratio Mask/Middle/Choices Box (1)").AddComponent<ObjectActivator>();
        fix.reactivateOnEnable = true;

        fix.events = new() { onActivate = new() };
        fix.events.onActivate.AddListener(() => fix.GetComponent<Voting>().enabled = true);
    }

    /// <summary> Changes the name of the character to Vermicelli. </summary>
    public static void Name(Text dialog, ref string name)
    {
        dialog.text = dialog.text.Replace("Mirage", "Vermicelli");

        var tex = dialog.font.material.mainTexture; // aspect ratio of the font texture must always be 1
        if (tex.width != tex.height) dialog.font.RequestCharactersInTexture("I love you", 512);

        dialog.font.RequestCharactersInTexture("3", 22);
        dialog.font.GetCharacterInfo('3', out var info, 22);

        name = name.Replace("MIRAGE:", $"V <quad size=18 x={info.uvBottomLeft.x:0.0000} y={info.uvBottomLeft.y:0.0000} width={info.uvTopRight.x - info.uvBottomLeft.x:0.0000} height={info.uvTopRight.y - info.uvBottomLeft.y:0.0000}> RMICELLI:".Replace(',', '.'));
    }

    #endregion
}
