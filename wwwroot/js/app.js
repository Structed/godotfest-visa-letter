window.visaLetter = (() => {
    // pdfmake table layouts must be functions, so they cannot travel through JSON from
    // the C# side. The document definition references this layout by name instead.
    const tableLayouts = {
        godotfest: {
            hLineWidth: (i, node) => (i === 0 || i === 1 || i === node.table.body.length) ? 0.8 : 0.4,
            vLineWidth: () => 0.4,
            hLineColor: (i, node) => (i === 0 || i === 1 || i === node.table.body.length) ? '#9fb3c6' : '#e0e6ed',
            vLineColor: () => '#e0e6ed',
            paddingTop: () => 4,
            paddingBottom: () => 4,
            paddingLeft: () => 7,
            paddingRight: () => 7,
            fillColor: (i) => (i === 0 ? '#eef3f8' : null)
        }
    };

    function build(json) {
        if (typeof pdfMake === 'undefined') {
            throw new Error('pdfmake failed to load — check wwwroot/lib/pdfmake.');
        }

        const dd = JSON.parse(json);
        const label = dd.footerLabel || 'Page';
        const of = dd.footerOf || 'of';
        delete dd.footerLabel;
        delete dd.footerOf;

        dd.footer = (currentPage, pageCount) => ({
            text: `${label} ${currentPage} ${of} ${pageCount}`,
            alignment: 'center',
            fontSize: 8,
            color: '#8b95a1',
            margin: [0, 20, 0, 0]
        });

        return pdfMake.createPdf(dd, tableLayouts);
    }

    // Mirrors the file-saver implementation that pdfmake itself bundles: a detached anchor,
    // clicked by dispatching a MouseEvent on the next tick. Appending the anchor and calling
    // .click() directly does not reliably start the download in Chrome.
    function saveBlob(blob, filename) {
        const link = document.createElement('a');
        link.download = filename;
        link.rel = 'noopener';
        link.href = URL.createObjectURL(blob);

        setTimeout(() => URL.revokeObjectURL(link.href), 40000);
        setTimeout(() => {
            try {
                link.dispatchEvent(new MouseEvent('click'));
            } catch {
                const evt = document.createEvent('MouseEvents');
                evt.initMouseEvent('click', true, true, window, 0, 0, 0, 80, 20,
                    false, false, false, false, 0, null);
                link.dispatchEvent(evt);
            }
        }, 0);
    }

    return {
        downloadPdf: (filename, json) => build(json).download(filename),

        openPdf: (json) => build(json).open(),

        // Returns the PDF as base64 so C# can bundle several files into one archive.
        pdfBase64: (json) => new Promise((resolve, reject) => {
            try {
                build(json).getBase64(resolve);
            } catch (err) {
                reject(err);
            }
        }),

        downloadText: (filename, text) => {
            saveBlob(new Blob([text], { type: 'text/markdown;charset=utf-8' }), filename);
        },

        downloadBase64: (filename, base64, mime) => {
            const binary = atob(base64);
            const bytes = new Uint8Array(binary.length);
            for (let i = 0; i < binary.length; i++) {
                bytes[i] = binary.charCodeAt(i);
            }

            saveBlob(new Blob([bytes], { type: mime || 'application/octet-stream' }), filename);
        },

        copyText: async (text) => {
            try {
                await navigator.clipboard.writeText(text);
                return true;
            } catch {
                return false;
            }
        }
    };
})();
