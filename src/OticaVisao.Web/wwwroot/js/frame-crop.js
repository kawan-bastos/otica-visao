(() => {
    const input = document.querySelector('input[type="file"][name="Input.Image"]');
    if (!input) return;
    const camera = document.createElement('input');
    camera.type = 'file';
    camera.accept = 'image/jpeg,image/png,image/webp';
    camera.setAttribute('capture', 'environment');
    camera.hidden = true;
    const cameraButton = document.createElement('button');
    cameraButton.type = 'button';
    cameraButton.className = 'button button--secondary frame-camera-button';
    cameraButton.textContent = 'Tirar foto';
    cameraButton.addEventListener('click', () => { camera.value = ''; camera.click(); });
    camera.addEventListener('change', () => {
        if (!camera.files.length) return;
        input.files = camera.files;
        input.dispatchEvent(new Event('change', {bubbles: true}));
    });
    input.after(cameraButton, camera);
    const editor = document.createElement('div');
    editor.className = 'frame-crop';
    editor.hidden = true;
    editor.innerHTML = `<h3>Enquadrar a armação</h3><p>Arraste a foto e ajuste o zoom. A área abaixo é a imagem que será salva.</p>
        <canvas width="1000" height="700" aria-label="Prévia do recorte da armação"></canvas>
        <label>Zoom<input type="range" data-zoom min="1" max="6" step="0.05" value="1"></label>
        <label>Posição horizontal<input type="range" data-x min="-1000" max="1000" value="0"></label>
        <label>Posição vertical<input type="range" data-y min="-700" max="700" value="0"></label>
        <button type="button" class="button button--secondary" data-reset>Centralizar</button>
        <button type="button" class="button button--primary" data-apply>Usar este recorte</button>
        <p role="status" aria-live="polite" data-status></p>`;
    input.after(editor);
    const canvas = editor.querySelector('canvas'), ctx = canvas.getContext('2d');
    const zoom = editor.querySelector('[data-zoom]'), x = editor.querySelector('[data-x]'), y = editor.querySelector('[data-y]');
    const status = editor.querySelector('[data-status]'), apply = editor.querySelector('[data-apply]');
    let photo, pending = false, generation = 0, drag;
    const changed = () => { pending = true; status.textContent = 'Confirme o recorte antes de salvar a armação.'; };
    function draw() {
        if (!photo) return;
        const scale = Math.min(1000 / photo.naturalWidth, 700 / photo.naturalHeight) * Number(zoom.value);
        const w = photo.naturalWidth * scale, h = photo.naturalHeight * scale;
        ctx.fillStyle = '#ffffff'; ctx.fillRect(0, 0, 1000, 700);
        ctx.drawImage(photo, (1000-w)/2 + Number(x.value), (700-h)/2 + Number(y.value), w, h);
    }
    input.addEventListener('change', async () => {
        const ticket = ++generation, file = input.files[0];
        pending = !!file; photo = null; editor.hidden = !file; apply.disabled = true;
        if (!file) return;
        status.textContent = 'Carregando foto…';
        if (file.size > 30 * 1024 * 1024 || !/image\/(jpeg|png|webp)/.test(file.type)) {
            input.value = ''; pending = false; status.textContent = 'Selecione uma foto JPG, PNG ou WebP de até 30 MB.'; return;
        }
        const url = URL.createObjectURL(file), img = new Image();
        try {
            img.src = url; await img.decode();
            if (ticket !== generation) return;
            photo = img; zoom.value = 1; x.value = y.value = 0; draw(); changed(); apply.disabled = false;
        } catch {
            if (ticket === generation) { input.value = ''; pending = false; status.textContent = 'Não foi possível abrir esta foto. Escolha outra imagem.'; }
        } finally { URL.revokeObjectURL(url); }
    });
    [zoom,x,y].forEach(control => control.addEventListener('input', () => { changed(); draw(); }));
    editor.querySelector('[data-reset]').addEventListener('click', () => { zoom.value = 1; x.value = y.value = 0; changed(); draw(); });
    canvas.addEventListener('pointerdown', e => { if (!photo) return; drag = {id:e.pointerId,x:e.clientX,y:e.clientY}; canvas.setPointerCapture(e.pointerId); });
    canvas.addEventListener('pointermove', e => {
        if (!drag || drag.id !== e.pointerId) return;
        const r = canvas.getBoundingClientRect();
        x.value = Number(x.value) + (e.clientX-drag.x)*1000/r.width;
        y.value = Number(y.value) + (e.clientY-drag.y)*700/r.height;
        drag.x=e.clientX; drag.y=e.clientY; changed(); draw();
    });
    ['pointerup','pointercancel','lostpointercapture'].forEach(event => canvas.addEventListener(event, () => { drag = null; }));
    apply.addEventListener('click', () => {
        if (!photo) return;
        draw(); const ticket = generation; apply.disabled = true;
        canvas.toBlob(blob => {
            if (ticket !== generation) return;
            try {
                if (!blob) throw new Error('encoding');
                const transfer = new DataTransfer();
                transfer.items.add(new File([blob], 'armacao-recortada.jpg', {type:'image/jpeg'}));
                input.files = transfer.files; pending = false;
                status.textContent = 'Recorte pronto! Agora salve a armação para concluir.';
            } catch { status.textContent = 'Não foi possível aplicar o recorte neste navegador. Tente novamente em um navegador atualizado.'; }
            apply.disabled = false;
        }, 'image/jpeg', .92);
    });
    input.form.addEventListener('submit', e => {
        if (pending) { e.preventDefault(); status.textContent = 'Clique em “Usar este recorte” antes de salvar.'; apply.focus(); }
    });
})();
