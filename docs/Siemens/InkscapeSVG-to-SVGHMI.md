# Inkscape SVG to SVGHMI Conversion for Siemens WinCC RT Unified

This document describes the planned feature for converting an SVG graphic created with
[Inkscape](https://inkscape.org/) into an **SVGHMI widget** compatible with
**Siemens WinCC RT Unified** (TIA Portal HMI panels).

## References

- [YouTube – SVG to SVGHMI demo](https://www.youtube.com/watch?v=YCGwq2Hzmn0)
- [SVGHMI.pro – WinCC Unified SVG to SVGHMI widget converter](https://svghmi.pro/blog/wincc-unified-svg-to-svghmi-widget-converter)

---

## Overview

WinCC RT Unified supports **SVGHMI** widgets: SVG files enriched with special attributes
and a scripting layer that bind graphical elements directly to PLC tags and HMI properties.
Currently, engineers must add these bindings by hand, which is tedious and error-prone.

The goal of this feature is to let an engineer open a plain Inkscape SVG file inside
VS Code and, with one click, be guided through the conversion into a fully configured
SVGHMI widget that can be imported into a WinCC RT Unified project.

---

## Conversion Concepts

### What Makes an SVG an SVGHMI

A standard Inkscape SVG becomes an SVGHMI widget by:

1. **Adding namespace declarations** – The `<svg>` root element gains the SVGHMI XML
   namespace (`xmlns:svghmi`) so the HMI runtime can identify the enriched elements.
2. **Tagging graphical elements** – Shapes, text labels, and groups receive
   `svghmi:property` attributes that map them to WinCC RT Unified HMI properties
   (e.g. `ForeColor`, `BackColor`, `Text`, `Visible`, `Width`, `Height`).
3. **Exposing interface properties** – A `<svghmi:interface>` block is inserted at the
   top of the SVG listing all HMI properties that the widget exposes to the parent screen.
   These are the properties visible when the widget is placed on a WinCC screen and
   its property panel is opened.
4. **Adding scripting hooks (optional)** – Inline ECMAScript references can be added to
   respond to value changes from the PLC at runtime.

### Typical SVG Elements and Their HMI Mappings

| Inkscape element          | Common HMI property       | Notes                                     |
|---------------------------|---------------------------|-------------------------------------------|
| `<rect>` / `<circle>`     | `FillColor`, `Visible`    | Background shapes, indicator lamps        |
| `<text>` / `<tspan>`      | `Text`, `ForeColor`       | Dynamic labels, numeric displays          |
| `<path>` (arrow/chevron)  | `Rotation`, `Visible`     | Direction indicators                      |
| `<g>` (named group)       | Multiple / sub-widget     | Groups can be promoted to sub-interfaces  |
| `<image>`                 | `Source`                  | Embedded bitmaps                          |

---

## Proposed VS Code Feature Design

### 1. Trigger: SVG File Open

When a `.svg` file is opened in VS Code the PlatformPLC extension will inspect its
content. If the file does **not** already contain the SVGHMI namespace attribute on
the `<svg>` root element, the extension treats the file as a *candidate for conversion*.

A **CodeLens item** labelled `⚙ Convert to SVGHMI Widget` will appear at the very top
of the text editor (line 1, before the XML declaration). Clicking this item opens the
conversion wizard.

```
⚙ Convert to SVGHMI Widget   ← CodeLens, visible at line 1
──────────────────────────────────────────────────────────────────
<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" ...>
  ...
</svg>
```

### 2. Conversion Wizard Webview

Clicking the CodeLens opens a **VS Code Webview panel** titled
*"SVGHMI Conversion Wizard"*. The wizard is divided into three steps.

#### Step 1 – Element Inspector

The wizard parses the SVG file and presents every named element (those with an `id`
attribute set in Inkscape) in a tree view. Each row shows:

- The element **ID** (as set in Inkscape's *Object Properties* dialog)
- The element **type** (`rect`, `text`, `path`, `g`, …)
- A **checkbox** to include or exclude the element from the HMI interface
- A **property selector** dropdown for the HMI property to bind
  (`FillColor`, `Text`, `Visible`, `Rotation`, …)
- An optional **tag path** text field to pre-fill the suggested WinCC tag binding

Elements without an `id` are listed in a separate *"Unnamed elements"* section and are
excluded by default, with a note encouraging the engineer to name elements in Inkscape
first.

#### Step 2 – Interface Property Configuration

For each element selected in Step 1, the engineer sees an editable table with:

| Column            | Description                                              |
|-------------------|----------------------------------------------------------|
| Property Name     | Name exposed in WinCC RT Unified's property panel        |
| Data Type         | `Bool`, `Int`, `Real`, `String`, …                       |
| Default Value     | Value used when no PLC tag is connected                  |
| Description       | Free-text description shown as tooltip in WinCC          |

The table is pre-populated with sensible defaults inferred from the element type
(e.g. a `<rect>` gets a `Bool` `Visible` property and a `String` `FillColor` property).

#### Step 3 – Preview & Export

A live SVG preview shows the widget with placeholder highlights for each bound element.
The engineer can choose between two export actions:

- **Save in-place** – Overwrites the current `.svg` file with the enriched SVGHMI
  version (namespace declarations, `<svghmi:interface>` block, and per-element
  `svghmi:property` attributes added).
- **Save as new file** – Saves the enriched widget alongside the original file with
  the suffix `_svghmi` (e.g. `MotorPanel_svghmi.svg`), keeping the Inkscape source
  untouched for future edits.

### 3. Post-Conversion Status Bar Item

After a successful conversion, the VS Code status bar shows a persistent item:

```
$(check) SVGHMI · 4 properties exported
```

Clicking the item reopens the wizard so the engineer can adjust property bindings at
any time.

---

## File Output Format (Illustrative Example)

Below is an illustrative example of what the enriched SVG header would look like after
conversion. The actual implementation may differ in exact attribute names.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg"
     xmlns:svghmi="https://svghmi.siemens.com/schema"
     width="200" height="100">

  <!-- SVGHMI Interface Block (generated by PlatformPLC) -->
  <svghmi:interface>
    <svghmi:property name="Running"     type="Bool"   default="false" description="Motor running state" />
    <svghmi:property name="SpeedValue"  type="Real"   default="0.0"   description="Motor speed in rpm"   />
    <svghmi:property name="StatusColor" type="String" default="#888"  description="Indicator fill color" />
    <svghmi:property name="LabelText"   type="String" default="---"   description="Name label"           />
  </svghmi:interface>

  <!-- Graphical elements with bound properties -->
  <rect id="indicator" x="10" y="10" width="30" height="30"
        svghmi:property="FillColor:StatusColor" />

  <text id="speedLabel" x="50" y="30"
        svghmi:property="Text:SpeedValue" />

  <text id="nameLabel" x="50" y="60"
        svghmi:property="Text:LabelText" />

</svg>
```

---

## Workflow Summary

```
┌─────────────────────────────────────────────────────────────────────┐
│ 1. Design widget in Inkscape                                        │
│    • Name every element using Object Properties (id attribute)      │
│    • Save as Plain SVG                                              │
└────────────────────────┬────────────────────────────────────────────┘
                         │ open .svg in VS Code
                         ▼
┌─────────────────────────────────────────────────────────────────────┐
│ 2. PlatformPLC detects SVG (no SVGHMI namespace)                    │
│    • CodeLens "⚙ Convert to SVGHMI Widget" appears at line 1        │
└────────────────────────┬────────────────────────────────────────────┘
                         │ click CodeLens
                         ▼
┌─────────────────────────────────────────────────────────────────────┐
│ 3. SVGHMI Conversion Wizard (Webview)                               │
│    Step 1 – Select elements and map to HMI properties               │
│    Step 2 – Configure property types, defaults, and descriptions    │
│    Step 3 – Preview and export (in-place or as new file)            │
└────────────────────────┬────────────────────────────────────────────┘
                         │ export
                         ▼
┌─────────────────────────────────────────────────────────────────────┐
│ 4. Enriched .svg ready for import into WinCC RT Unified             │
│    • Status bar item confirms export and offers re-open wizard      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Design Considerations

- **Non-destructive by default** – The "save as new file" default ensures the Inkscape
  source is preserved for future design iterations.
- **Incremental conversion** – Re-opening an already-converted SVG shows the wizard
  pre-filled with the existing bindings, allowing engineers to add or modify properties
  without starting from scratch.
- **No vendor lock-in at the SVG level** – The SVGHMI attributes live in a separate
  namespace; the file remains a valid, renderable SVG in any browser or viewer.
- **Unnamed elements** – Engineers are guided to name elements in Inkscape first; the
  wizard surface-levels this requirement clearly so that the quality of the input SVG
  is improved before conversion.
- **Future extension** – The wizard architecture is designed so that additional HMI
  platforms (e.g. Rockwell FactoryTalk Optix) can be added as conversion targets
  without changing the wizard UI.
