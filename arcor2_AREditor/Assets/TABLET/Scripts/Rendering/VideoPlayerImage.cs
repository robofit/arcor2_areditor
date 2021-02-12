using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerImage : MonoBehaviour {

    public RawImage RawImage;
    public VideoPlayer VideoPlayer;
    
    private Texture originalImageTexture;

    private void Start() {
        originalImageTexture = RawImage.texture;
        VideoPlayer.prepareCompleted += OnVideoPrepared;
    }

    // Puts texture on Image when the video is ready to play
    private void OnVideoPrepared(VideoPlayer source) {
        RawImage.texture = source.texture;
    }

    public void PlayVideo() {
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.Disconnected ||
            GameManager.Instance.GetGameState() == GameManager.GameStateEnum.MainScreen)
            return;
        // Play() will call function Prepare() which triggers event prepareCompleted
        VideoPlayer.Play();
    }

    public void PlayVideo(float playingTime) {
        if (GameManager.Instance.GetGameState() == GameManager.GameStateEnum.Disconnected ||
            GameManager.Instance.GetGameState() == GameManager.GameStateEnum.MainScreen)
            return;
        VideoPlayer.Play();
        Invoke("StopVideo", playingTime);
    }

    public void StopVideo() {
        VideoPlayer.Stop();
        RawImage.texture = originalImageTexture;
    }

}
