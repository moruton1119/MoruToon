# Changelog

## [0.1.0] - Initial Release

### Features

#### Particle Features
- **UV Scroll** - Animated texture scrolling with adjustable U/V speed
- **Rotation** - UV rotation animation for magic circles, spinning effects
- **Emission** - HDR emission with pulse animation support
- **Flipbook** - Texture sheet animation with optional frame blending
- **Soft Particle** - Distance-based alpha fade to prevent mesh clipping
- **Color Ramp** - Gradient texture-based color control
- **HUE Shift** - Real-time hue rotation (manual or auto-animated)
- **Lifetime Fade** - Particle age-based fade in/out (requires Custom Vertex Streams)
- **Layer Blend** - Multi-texture blending (Add/Multiply/Screen/Overlay)

#### Gimmick Features
- **Dissolve** - Noise-based dissolve with glowing edge effect
- **Lifetime Dissolve** - Auto-dissolve based on particle lifetime
- **Mask** - Visible/Hide mask textures for selective rendering
- **Stencil** - Portal masking and cutout effects
- **Distance Fade** - Camera distance-based transparency

#### System Features
- **Template Tab System** - 8 preset templates with one-click switching
  - Basic, UV Scroll, Dissolve, Flipbook, Layer Blend, Soft Particle, Stencil Portal, Particle FX, Custom
- **Full Rendering Control** - Blend mode, ZWrite, Cull mode all switchable
- **shader_feature Optimization** - Unused features compiled out for zero overhead
- **Custom Inspector** - Clean tabbed UI, no external dependencies
- **VCC Distribution** - One-click install via VRChat Creator Companion
