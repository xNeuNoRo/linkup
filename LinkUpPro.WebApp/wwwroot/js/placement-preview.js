// LinkUpPro - Battleship placement preview (client-side validation + ghost cells)
(function () {
    'use strict';

    var pickX = null, pickY = null, selectedSize = 0;
    var activeDir = -1;
    var board = document.getElementById('placement-board');
    var directionPicker = document.getElementById('direction-picker');
    var instructionEl = document.getElementById('placement-instruction');
    var confirmBtn = document.getElementById('btn-confirm');
    var cancelBtn = document.getElementById('cancel-placement');
    var gameId = 0;

    if (!board) return;
    var gameIdAttr = document.querySelector('input[name="GameId"]');
    if (gameIdAttr) gameId = parseInt(gameIdAttr.value, 10);

    var token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

    // Remove default active highlight — JS manages it exclusively
    document.querySelectorAll('.fleet-ship-btn').forEach(function (b) { b.classList.remove('fleet-ship-btn-active'); });

    function getOccupiedCells() {
        var cells = [];
        board.querySelectorAll('.battleship-cell.ship').forEach(function (el) {
            var x = parseInt(el.dataset.x, 10);
            var y = parseInt(el.dataset.y, 10);
            if (!isNaN(x) && !isNaN(y)) cells.push({ x: x, y: y });
        });
        return cells;
    }

    function getShipCells(startX, startY, size, dir) {
        var cells = [];
        for (var i = 0; i < size; i++) {
            var x = startX, y = startY;
            if (dir === 0) y -= i;       // Up
            else if (dir === 1) y += i;   // Down
            else if (dir === 2) x -= i;   // Left
            else if (dir === 3) x += i;   // Right
            cells.push({ x: x, y: y });
        }
        return cells;
    }

    function isPlacementValid(cells) {
        var occupied = getOccupiedCells();
        for (var i = 0; i < cells.length; i++) {
            var c = cells[i];
            if (c.x < 0 || c.x >= 12 || c.y < 0 || c.y >= 12) return false;
            for (var j = 0; j < occupied.length; j++) {
                if (occupied[j].x === c.x && occupied[j].y === c.y) return false;
            }
        }
        return true;
    }

    function clearGhosts() {
        board.querySelectorAll('.preview-ghost').forEach(function (el) {
            el.classList.remove('preview-ghost', 'dir-active', 'invalid');
        });
        board.querySelectorAll('.dir-arrow-btn').forEach(function (b) { b.classList.remove('active'); });
        if (confirmBtn) { confirmBtn.disabled = true; confirmBtn.classList.add('opacity-50', 'cursor-not-allowed'); }
        activeDir = -1;
    }

    function paintGhost(allCells, results) {
        clearGhosts();
        for (var i = 0; i < allCells.length; i++) {
            var group = allCells[i];
            var valid = results[i];
            for (var j = 0; j < group.length; j++) {
                var c = group[j];
                if (c.x < 0 || c.x >= 12 || c.y < 0 || c.y >= 12) continue;
                var el = board.querySelector('.placement-cell[data-x="' + c.x + '"][data-y="' + c.y + '"]');
                if (el && !el.classList.contains('ship')) {
                    el.classList.add('preview-ghost');
                    if (!valid) el.classList.add('invalid');
                }
            }
        }
    }

    function showDirectionPreviews(startX, startY, size) {
        pickX = startX; pickY = startY; selectedSize = size;
        var allCells = [], results = [];
        var dirs = [
            { dir: 0 }, { dir: 1 }, { dir: 2 }, { dir: 3 }
        ];
        for (var i = 0; i < dirs.length; i++) {
            var cells = getShipCells(startX, startY, size, dirs[i].dir);
            allCells.push(cells);
            results.push(isPlacementValid(cells));
        }
        paintGhost(allCells, results);

        // Update direction buttons with valid/invalid state
        document.querySelectorAll('.dir-arrow-btn').forEach(function (btn, i) {
            if (i < results.length) {
                btn.disabled = false;
                btn.classList.toggle('opacity-50', !results[i]);
            }
        });

        directionPicker.classList.remove('hidden');
        if (instructionEl) {
            instructionEl.innerHTML = '<i data-lucide="compass" class="w-3 h-3 inline"></i> Elige dirección. Verde = válida, rojo = inválida.';
            if (window.lucide) lucide.createIcons();
        }
    }

    function selectDirection(dir) {
        clearGhosts();
        activeDir = dir;
        var cells = getShipCells(pickX, pickY, selectedSize, dir);
        var valid = isPlacementValid(cells);

        // Paint selected direction ghost
        for (var j = 0; j < cells.length; j++) {
            var c = cells[j];
            if (c.x < 0 || c.x >= 12 || c.y < 0 || c.y >= 12) continue;
            var el = board.querySelector('.placement-cell[data-x="' + c.x + '"][data-y="' + c.y + '"]');
            if (el && !el.classList.contains('ship')) {
                el.classList.add('preview-ghost', 'dir-active');
                if (!valid) el.classList.add('invalid');
            }
        }

        // Highlight active arrow
        document.querySelectorAll('.dir-arrow-btn').forEach(function (b) {
            b.classList.remove('active');
            var d = parseInt(b.dataset.dir, 10);
            if (d === dir) b.classList.add('active');
        });

        if (confirmBtn) {
            confirmBtn.disabled = !valid;
            confirmBtn.classList.toggle('opacity-50', !valid);
            confirmBtn.classList.toggle('cursor-not-allowed', !valid);
        }
        if (instructionEl) {
            instructionEl.innerHTML = valid
                ? '<i data-lucide="check-circle" class="w-3 h-3 inline text-green-600"></i> Posición válida. Presiona <strong>Posicionar</strong> para confirmar.'
                : '<i data-lucide="x-circle" class="w-3 h-3 inline text-red-600"></i> Posición inválida: el barco sale del tablero o se superpone.';
            if (window.lucide) lucide.createIcons();
        }
    }

    function cancelPlacement() {
        clearGhosts();
        pickX = null; pickY = null;
        // Don't reset selectedSize or active highlight — keep the selected ship
        directionPicker.classList.add('hidden');
        if (instructionEl) {
            var label = document.querySelector('.fleet-ship-btn-active')?.dataset?.label || 'barco';
            instructionEl.innerHTML = '<i data-lucide="mouse-pointer-2" class="w-3 h-3 inline"></i> Coloca <strong>' + label + '</strong> en el tablero.';
            if (window.lucide) lucide.createIcons();
        }
    }

    // Cell click → show direction previews
    board.addEventListener('click', function (e) {
        var cell = e.target.closest('.placement-cell');
        if (!cell || cell.disabled || cell.classList.contains('ship')) return;
        if (!selectedSize) {
            if (instructionEl) {
                instructionEl.innerHTML = '<i data-lucide="alert-circle" class="w-3 h-3 inline text-amber-600"></i> Primero selecciona un barco de la lista.';
                if (window.lucide) lucide.createIcons();
            }
            return;
        }
        var x = parseInt(cell.dataset.x, 10);
        var y = parseInt(cell.dataset.y, 10);
        if (isNaN(x) || isNaN(y)) return;
        showDirectionPreviews(x, y, selectedSize);
    });

    // Direction arrow button click — delegated from document since buttons are outside board
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.dir-arrow-btn');
        if (!btn || btn.disabled) return;
        // Ignore clicks on buttons still disabled
        if (btn.classList.contains('opacity-50')) return;
        selectDirection(parseInt(btn.dataset.dir, 10));
    });

    // Confirm placement
    if (confirmBtn) {
        confirmBtn.addEventListener('click', function () {
            if (this.disabled || activeDir < 0 || !selectedSize || pickX === null || pickY === null) return;
            var formData = new URLSearchParams();
            formData.append('SelectedShipSize', selectedSize);
            formData.append('StartX', pickX);
            formData.append('StartY', pickY);
            formData.append('Direction', activeDir);
            formData.append('GameId', gameId);

            this.disabled = true;
            this.innerHTML = '<i data-lucide="loader-circle" class="w-4 h-4 animate-spin"></i>';
            if (window.lucide) lucide.createIcons();

            fetch('/Battleship/PlaceShip', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: formData.toString()
            })
            .then(function (r) {
                if (r.redirected) {
                    window.location.href = r.url;
                } else if (r.ok) {
                    window.location.href = '/Battleship/Placement?gameId=' + gameId;
                } else {
                    throw new Error();
                }
            })
            .catch(function () {
                window.location.href = '/Battleship/Placement?gameId=' + gameId;
            });
        });
    }

    // Cancel
    if (cancelBtn) cancelBtn.addEventListener('click', cancelPlacement);

    // Ship selection → update visual active state + reset preview
    document.querySelectorAll('.fleet-ship-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            document.querySelectorAll('.fleet-ship-btn').forEach(function (b) {
                b.classList.remove('fleet-ship-btn-active');
            });
            btn.classList.add('fleet-ship-btn-active');
            selectedSize = parseInt(btn.dataset.size, 10);
            cancelPlacement();
            if (window.lucide) lucide.createIcons();
        });
    });

    // Keyboard shortcuts
    document.addEventListener('keydown', function (e) {
        if (directionPicker.classList.contains('hidden')) return;
        var dirMap = {
            ArrowDown: 0, ArrowUp: 1, ArrowRight: 2, ArrowLeft: 3,
            s: 0, w: 1, d: 2, a: 3
        };
        var dir = dirMap[e.key] ?? (dirMap[e.key.toLowerCase()] !== undefined ? dirMap[e.key.toLowerCase()] : undefined);
        if (dir !== undefined) {
            e.preventDefault();
            var btn = document.querySelector('.dir-arrow-btn[data-dir="' + dir + '"]');
            if (btn && !btn.classList.contains('opacity-50')) selectDirection(dir);
        }
        if (e.key === 'Enter' && confirmBtn && !confirmBtn.disabled) confirmBtn.click();
        if (e.key === 'Escape') cancelPlacement();
    });

})();
