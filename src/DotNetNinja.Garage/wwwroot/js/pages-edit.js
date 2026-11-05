(() => {
  const editor = document.querySelector('.pages-editing[data-move-url]');
  if (!editor) return;

  // Group cards use backdrop-filter, which would confine nested fixed dialogs.
  editor.querySelectorAll('.garage-delete-dialog').forEach(dialog => editor.append(dialog));

  if (editor.dataset.initialDialog) {
    const dialog = document.getElementById(editor.dataset.initialDialog);
    if (dialog) {
      bootstrap.Modal.getOrCreateInstance(dialog).show();
      dialog.addEventListener('hidden.bs.modal', () => {
        const url = new URL(window.location.href);
        ['editLinkId', 'newLinkGroupId', 'editGroupId'].forEach(key => url.searchParams.delete(key));
        window.history.replaceState(null, '', url);
      });
    }
  }

  const status = document.getElementById('edit-move-status');
  const token = editor.querySelector('input[name="__RequestVerificationToken"]')?.value;
  const iconInput = document.getElementById('edit-link-icon');
  const colorInput = document.getElementById('edit-link-color');
  const iconPreview = document.getElementById('edit-icon-preview');
  let source = null;
  let groupSource = null;
  let groupTarget = null;
  let targetGroup = null;
  let targetLink = null;
  let pointer = null;
  let saving = false;

  function updateIconPreview() {
    if (!iconInput || !iconPreview) return;
    const option = [...document.getElementById('garage-icons').options]
      .find(item => item.value.toLowerCase() === iconInput.value.toLowerCase());
    iconPreview.className = `bi bi-${option?.dataset.cssName || 'link-45deg'}`;
    iconPreview.style.color = /^#[0-9a-f]{6}$/i.test(colorInput.value) ? colorInput.value : '#fff';
  }
  iconInput?.addEventListener('input', updateIconPreview);
  colorInput?.addEventListener('input', updateIconPreview);
  updateIconPreview();

  function announce(message, isError = false) {
    status.textContent = message;
    status.classList.toggle('visually-hidden', !isError);
    status.classList.toggle('alert', isError);
    status.classList.toggle('alert-danger', isError);
  }

  function clearHighlight() {
    editor.querySelectorAll('.drop-target, .drop-before, .drop-after, .drag-source, .group-drop-before, .group-drop-after')
      .forEach(element => element.classList.remove('drop-target', 'drop-before', 'drop-after', 'drag-source', 'group-drop-before', 'group-drop-after'));
    targetGroup = null;
    targetLink = null;
    groupTarget = null;
  }

  function groupDropAfter(target, clientX, clientY) {
    const bounds = target.getBoundingClientRect();
    return clientY > bounds.bottom || (clientY >= bounds.top && clientX > bounds.left + bounds.width / 2);
  }

  function highlightGroup(element, clientX, clientY) {
    editor.querySelectorAll('.group-drop-before, .group-drop-after')
      .forEach(item => item.classList.remove('group-drop-before', 'group-drop-after'));
    groupTarget = element?.closest('[data-group-id]') ?? null;
    if (!groupTarget || groupTarget === groupSource) {
      groupTarget = null;
      return;
    }
    groupTarget.classList.add(groupDropAfter(groupTarget, clientX, clientY) ? 'group-drop-after' : 'group-drop-before');
  }

  async function saveGroupMove(clientX, clientY) {
    if (!groupSource || !groupTarget || !token || saving) {
      clearHighlight();
      groupSource = null;
      return;
    }
    const grid = groupSource.parentElement;
    const groups = [...grid.querySelectorAll(':scope > [data-group-id]')];
    const currentIndex = groups.indexOf(groupSource);
    const siblings = groups.filter(group => group !== groupSource);
    const position = siblings.indexOf(groupTarget) + (groupDropAfter(groupTarget, clientX, clientY) ? 1 : 0);
    if (position === currentIndex) {
      clearHighlight();
      groupSource = null;
      return;
    }

    const form = new FormData();
    form.set('__RequestVerificationToken', token);
    form.set('id', groupSource.dataset.groupId);
    form.set('revision', groupSource.dataset.groupRevision);
    form.set('position', position.toString());
    saving = true;
    editor.classList.add('is-saving');
    announce('Saving group position.');
    try {
      const response = await fetch(editor.dataset.groupMoveUrl, {
        method: 'POST', body: form, credentials: 'same-origin', headers: { Accept: 'application/json' }
      });
      if (!response.ok) {
        const details = await response.json().catch(() => null);
        announce(details?.message ?? 'The group could not be moved. Reload and try again.', true);
        return;
      }
      announce('Group moved.');
      window.location.reload();
    } catch {
      announce('The group could not be moved. Check your connection and try again.', true);
    } finally {
      saving = false;
      editor.classList.remove('is-saving');
      clearHighlight();
      groupSource = null;
    }
  }

  function highlight(element, clientY) {
    editor.querySelectorAll('.drop-target, .drop-before, .drop-after')
      .forEach(item => item.classList.remove('drop-target', 'drop-before', 'drop-after'));
    targetGroup = element?.closest('[data-group-id]') ?? null;
    targetLink = element?.closest('li[data-link-id]') ?? null;
    if (!targetGroup || targetLink === source) {
      targetGroup = null;
      targetLink = null;
      return;
    }
    targetGroup.classList.add('drop-target');
    if (targetLink) {
      const after = clientY > targetLink.getBoundingClientRect().top + targetLink.getBoundingClientRect().height / 2;
      targetLink.classList.add(after ? 'drop-after' : 'drop-before');
    }
  }

  async function saveMove(clientY) {
    if (!source || !targetGroup || !token || saving) {
      clearHighlight();
      return;
    }
    const sourceGroup = source.closest('[data-group-id]');
    const siblings = [...targetGroup.querySelectorAll(':scope > [data-link-list] > li[data-link-id]')]
      .filter(item => item !== source);
    const targetIndex = siblings.indexOf(targetLink);
    const after = targetLink && clientY > targetLink.getBoundingClientRect().top + targetLink.getBoundingClientRect().height / 2;
    const position = targetIndex < 0 ? siblings.length : targetIndex + (after ? 1 : 0);
    const previousIndex = [...sourceGroup.querySelectorAll(':scope > [data-link-list] > li[data-link-id]')].indexOf(source);
    if (sourceGroup === targetGroup && position === previousIndex) {
      clearHighlight();
      return;
    }

    const form = new FormData();
    form.set('__RequestVerificationToken', token);
    form.set('LinkId', source.dataset.linkId);
    form.set('LinkRevision', source.dataset.linkRevision);
    form.set('SourceGroupId', sourceGroup.dataset.groupId);
    form.set('SourceRevision', sourceGroup.dataset.groupRevision);
    form.set('DestinationGroupId', targetGroup.dataset.groupId);
    form.set('DestinationRevision', targetGroup.dataset.groupRevision);
    form.set('Position', position.toString());

    saving = true;
    editor.classList.add('is-saving');
    announce('Saving link position.');
    try {
      const response = await fetch(editor.dataset.moveUrl, {
        method: 'POST',
        body: form,
        credentials: 'same-origin',
        headers: { Accept: 'application/json' }
      });
      if (!response.ok) {
        const details = await response.json().catch(() => null);
        announce(details?.message ?? 'The move could not be saved. Reload and try again.', true);
        return;
      }
      announce('Link moved.');
      window.location.reload();
    } catch {
      announce('The move could not be saved. Check your connection and try again.', true);
    } finally {
      saving = false;
      editor.classList.remove('is-saving');
      clearHighlight();
      source = null;
    }
  }

  editor.addEventListener('pointerdown', event => {
    if (saving || (event.pointerType === 'mouse' && event.button !== 0)) return;
    const groupHandle = event.target.closest('[data-group-drag-handle]');
    if (groupHandle) {
      groupSource = groupHandle.closest('[data-group-id]');
      pointer = event.pointerId;
      groupHandle.setPointerCapture(pointer);
      groupSource.classList.add('drag-source');
      document.body.classList.add('is-dragging-link');
      return;
    }
    const handle = event.target.closest('[data-drag-handle]');
    if (!handle) return;
    source = handle.closest('li[data-link-id]');
    pointer = event.pointerId;
    handle.setPointerCapture(pointer);
    source.classList.add('drag-source');
    document.body.classList.add('is-dragging-link');
  });
  editor.addEventListener('pointermove', event => {
    if (pointer !== event.pointerId) return;
    const element = document.elementFromPoint(event.clientX, event.clientY);
    if (groupSource) highlightGroup(element, event.clientX, event.clientY);
    else if (source) highlight(element, event.clientY);
  });
  editor.addEventListener('pointerup', event => {
    if (pointer !== event.pointerId) return;
    const element = document.elementFromPoint(event.clientX, event.clientY);
    if (groupSource) highlightGroup(element, event.clientX, event.clientY);
    else highlight(element, event.clientY);
    pointer = null;
    document.body.classList.remove('is-dragging-link');
    if (groupSource) void saveGroupMove(event.clientX, event.clientY);
    else void saveMove(event.clientY);
  });
  editor.addEventListener('pointercancel', event => {
    if (pointer !== event.pointerId) return;
    pointer = null;
    document.body.classList.remove('is-dragging-link');
    clearHighlight();
    source = null;
    groupSource = null;
  });

})();
