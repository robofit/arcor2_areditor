using System;
using System.Collections;
using System.Collections.Generic;
using OrbCreationExtensions;
using UnityEngine;
using UnityEngine.EventSystems;

public class TransformWheelList : EventTrigger {
    private Vector2 _curPosition;
    private Vector2 _velocity;
    private bool _underInertia;
    private bool _finishing;
    private bool _dragging = false;
    private float _time = 0.0f;
    private float _smoothTime = 2f;
    private float _finishY;

    public void Init() {
        _underInertia = false;
        _dragging = false;
        _finishing = false;
        _time = 0;
        _finishY = 0;
        _velocity = Vector2.zero;
    }

    public void Update() {

        if (_dragging) {
            Vector2 prevPosition = _curPosition;
            _curPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            _velocity = _curPosition - prevPosition;
            transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y)
                + new Vector2(0, _velocity.y);
            return;
        }
        if (_underInertia) {
            if (_time <= _smoothTime && Mathf.Abs(_velocity.y) > 0.1f) {
                transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y)
                    + new Vector2(0, _velocity.y);
                _velocity = Vector2.Lerp(_velocity, Vector2.zero, _time);
                
                _time += Time.smoothDeltaTime;
            } else {
                _underInertia = false;
                _finishing = true;
                _time = 0.0f;
                _finishY = ClosestInteger((int) transform.localPosition.y, 80);
            }
        }

        if (_finishing) {
            if (_time <= 0.5f) {
                transform.localPosition = Vector2.Lerp(transform.localPosition, new Vector2(transform.localPosition.x, _finishY), 0.5f);
                //transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y)
                //    + new Vector2(0, _velocity.y);
                _time += Time.smoothDeltaTime;
            } else {
                _time = 0.0f;
                _finishing = false;
            }
        }
        
    }

    private int ClosestInteger(int n, int m) {
        int q = n / m;

        // 1st possible closest number 
        int n1 = m * q;

        // 2nd possible closest number 
        int n2 = (n * m) > 0 ? (m * (q + 1)) : (m * (q - 1));

        // if true, then n1 is the required closest number 
        if (Math.Abs(n - n1) < Math.Abs(n - n2))
            return n1;

        // else n2 is the required closest number 
        return n2;
    }

    public override void OnPointerDown(PointerEventData eventData) {
        _curPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        _underInertia = false;
        _dragging = true;
        _time = 0.0f;
    }

    public override void OnPointerUp(PointerEventData eventData) {
        _underInertia = true;
        _dragging = false;
    }
}
