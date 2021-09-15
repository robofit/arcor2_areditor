using System;
using System.Collections;
using System.Collections.Generic;
using OrbCreationExtensions;
using UnityEngine;
using UnityEngine.EventSystems;

public class TransformWheelList : EventTrigger {
    private Vector2 _curPosition;
    public Vector2 Velocity;
    private bool _underInertia;
    private bool _finishing;
    private float _time = 0.0f;
    private float _smoothTime = 2f;
    private float _finishY;

    public bool Dragging { get; private set; } = false;

    public EventHandler MovementDone, MovementStart;

    public void Init() {
        if (Velocity != Vector2.zero)
            MovementDone?.Invoke(this, EventArgs.Empty);
        _underInertia = false;
        Dragging = false;
        _finishing = false;
        _time = 0;
        _finishY = 0;
        Velocity = Vector2.zero;
    }

    public void Update() {

        if (Dragging) {
            Vector2 prevPosition = _curPosition;
            _curPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Velocity = _curPosition - prevPosition;
            transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y)
                + new Vector2(0, Velocity.y);
            return;
        }
        if (_underInertia) {
            if (_time <= _smoothTime && Mathf.Abs(Velocity.y) > 0.1f) {
                transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y)
                    + new Vector2(0, Velocity.y);
                Velocity = Vector2.Lerp(Velocity, Vector2.zero, _time);

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
                Velocity = Vector2.zero;
                MovementDone?.Invoke(this, EventArgs.Empty);
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
        if (Velocity.magnitude == 0) {
            MovementStart?.Invoke(this, EventArgs.Empty);
        }
        _curPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        _underInertia = false;
        Dragging = true;
        _time = 0.0f;
        Velocity = Vector2.zero;
        _finishing = false;
    }

    public override void OnPointerUp(PointerEventData eventData) {
        _underInertia = true;
        Dragging = false;
    }

    public void Stop() {
        if (Dragging)
            return;
        _underInertia = false;
        _time = 0.0f;
        Velocity = Vector2.zero;_finishing = false;
    }
}
