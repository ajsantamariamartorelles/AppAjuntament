window.galeriaFotosLightbox = (function () {
    var _zoom = 1;
    var _panX = 0;
    var _panY = 0;
    var _dragging = false;
    var _startX = 0;
    var _startY = 0;
    var _startPanX = 0;
    var _startPanY = 0;
    var _moved = false;
    var _dotNet = null;
    var _wrap = null;
    var _img = null;

    function applyTransform() {
        if (!_img) return;
        _img.style.transformOrigin = 'center center';
        _img.style.transform = 'translate(' + _panX + 'px, ' + _panY + 'px) scale(' + _zoom + ')';
        if (_wrap) {
            _wrap.classList.toggle('zoomed', _zoom > 1.01);
        }
    }

    function onMouseDown(e) {
        if (e.button !== 0) return;
        if (e.target.closest('button')) return;
        _dragging = true;
        _moved = false;
        _startX = e.clientX;
        _startY = e.clientY;
        _startPanX = _panX;
        _startPanY = _panY;
        e.preventDefault();
    }

    function onMouseMove(e) {
        if (!_dragging) return;
        var dx = e.clientX - _startX;
        var dy = e.clientY - _startY;
        if (Math.abs(dx) > 3 || Math.abs(dy) > 3) _moved = true;
        if (_zoom > 1.01) {
            _panX = _startPanX + dx;
            _panY = _startPanY + dy;
            applyTransform();
        }
    }

    function onMouseUp(e) {
        if (!_dragging) return;
        _dragging = false;
        if (!_moved) {
            if (_zoom > 1.01) {
                _zoom = 1;
                _panX = 0;
                _panY = 0;
            } else {
                _zoom = 2.5;
            }
            applyTransform();
            notifyBlazor();
        }
    }

    function onWheel(e) {
        e.preventDefault();
        var delta = e.deltaY < 0 ? 0.25 : -0.25;
        var newZoom = Math.max(1.0, Math.min(5.0, _zoom + delta));
        if (newZoom !== _zoom) {
            if (newZoom <= 1.0) {
                _panX = 0;
                _panY = 0;
            }
            _zoom = newZoom;
            applyTransform();
            notifyBlazor();
        }
    }

    function notifyBlazor() {
        if (_dotNet) {
            _dotNet.invokeMethodAsync('SetZoomLevel', _zoom).catch(function () { });
        }
    }

    function cleanup() {
        if (_wrap) {
            _wrap.removeEventListener('mousedown', onMouseDown);
            _wrap.removeEventListener('wheel', onWheel);
        }
        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup', onMouseUp);
    }

    return {
        init: function (dotNetRef) {
            cleanup();
            _dotNet = dotNetRef;
            _zoom = 1;
            _panX = 0;
            _panY = 0;
            _dragging = false;
            _wrap = document.querySelector('.lightbox-img-wrap');
            _img = document.querySelector('.lightbox-img');
            if (!_wrap || !_img) return;
            applyTransform();
            _wrap.addEventListener('mousedown', onMouseDown);
            _wrap.addEventListener('wheel', onWheel, { passive: false });
            document.addEventListener('mousemove', onMouseMove);
            document.addEventListener('mouseup', onMouseUp);
        },

        setZoom: function (zoom) {
            _zoom = Math.max(1.0, Math.min(5.0, zoom));
            if (_zoom <= 1.0) {
                _panX = 0;
                _panY = 0;
            }
            applyTransform();
        },

        resetZoom: function () {
            _zoom = 1;
            _panX = 0;
            _panY = 0;
            applyTransform();
        },

        destroy: function () {
            cleanup();
            _dotNet = null;
            _wrap = null;
            _img = null;
        }
    };
})();
