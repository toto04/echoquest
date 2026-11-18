#!/usr/bin/env bash
# Publishes the tutor zips made by tools/build_demo.sh as a GitHub release:
#
#   tag demo-<YYYY-MM-DD>, title "EchoQuest demo <YYYY-MM-DD>", one asset per OS,
#   tagged at the commit the zips were built from (read from their BUILD_INFO.txt).
#
# Usage (from anywhere in the repo; needs gh, logged in):
#   tools/publish_demo.sh                  # the newest date in Builds/demo/
#   tools/publish_demo.sh 2026-11-18       # a given date
#   tools/publish_demo.sh --dry-run        # check everything, print the notes, publish nothing
#   tools/publish_demo.sh --replace        # that date's release exists: delete it and its tag first
#   tools/publish_demo.sh --yes            # no confirmation prompt
#   tools/publish_demo.sh --allow-dirty    # zips built with --allow-dirty: published as a pre-release
#
# Build and publish in one go:  tools/build_demo.sh && tools/publish_demo.sh --yes
# Details: docs/demo/desktop-builds.md
set -euo pipefail

REPO="$(git -C "$(dirname "$0")" rev-parse --show-toplevel)"
OUT="$REPO/Builds/demo"
DATE="" DRY_RUN=0 REPLACE=0 YES=0 ALLOW_DIRTY=0

for arg in "$@"; do
  case "$arg" in
    --dry-run) DRY_RUN=1 ;;
    --replace) REPLACE=1 ;;
    --yes|-y) YES=1 ;;
    --allow-dirty) ALLOW_DIRTY=1 ;;
    -h|--help) sed -n '2,19p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    [0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]) DATE="$arg" ;;
    *) echo "Unknown argument: $arg (see --help)" >&2; exit 2 ;;
  esac
done

die() { echo "error: $*" >&2; exit 1; }
cd "$REPO"

command -v gh >/dev/null || die "GitHub CLI not found: brew install gh"
gh auth status >/dev/null 2>&1 || die "gh is not logged in: gh auth login"
command -v unzip >/dev/null || die "unzip not found"
GH_REPO="$(gh repo view --json nameWithOwner -q .nameWithOwner)"

# --- Pick the zips ------------------------------------------------------------

if [ -z "$DATE" ]; then
  DATE="$(ls "$OUT" 2>/dev/null | sed -n 's/^echoquest_demo_[a-z]*_\([0-9-]\{10\}\)\.zip$/\1/p' | sort -u | tail -n 1)"
  [ -n "$DATE" ] || die "no echoquest_demo_*.zip in ${OUT#$REPO/}/. Run tools/build_demo.sh first."
fi

ZIPS=() MISSING=()
for os in linux macos windows; do
  f="$OUT/echoquest_demo_${os}_${DATE}.zip"
  if [ -f "$f" ]; then ZIPS+=("$f"); else MISSING+=("$os"); fi
done
[ ${#ZIPS[@]} -gt 0 ] || die "no zips for $DATE in ${OUT#$REPO/}/"

# --- Commit they were built from ---------------------------------------------

COMMIT_LINE=""
for z in "${ZIPS[@]}"; do
  base="$(basename "$z" .zip)"
  line="$(unzip -p "$z" "$base/BUILD_INFO.txt" 2>/dev/null | sed -n 's/^Commit: *//p')"
  [ -n "$line" ] || die "$(basename "$z") has no BUILD_INFO.txt with a commit. Rebuild it with tools/build_demo.sh."
  if [ -z "$COMMIT_LINE" ]; then COMMIT_LINE="$line"
  elif [ "$line" != "$COMMIT_LINE" ]; then
    die "the zips for $DATE come from different commits ('$COMMIT_LINE' and '$line'). Rebuild them together."
  fi
done

DIRTY=0
case "$COMMIT_LINE" in *uncommitted*) DIRTY=1 ;; esac
if [ "$DIRTY" -eq 1 ] && [ "$ALLOW_DIRTY" -eq 0 ]; then
  die "the zips were built with uncommitted changes, so no commit matches them. Commit and rebuild, or pass --allow-dirty (published as a pre-release)."
fi

SHORT="${COMMIT_LINE%% *}"
SHA="$(git rev-parse --verify --quiet "$SHORT^{commit}")" || die "commit $SHORT is not in this clone"
gh api "repos/$GH_REPO/commits/$SHA" --silent 2>/dev/null \
  || die "commit $SHORT is not on GitHub yet. Push it (git push), then run this again."
SUBJECT="$(git log -1 --format=%s "$SHA")"

# --- Existing release and the previous one -----------------------------------

TAG="demo-$DATE"
RELEASE_EXISTS=0 TAG_EXISTS=0
gh release view "$TAG" --repo "$GH_REPO" >/dev/null 2>&1 && RELEASE_EXISTS=1
[ -n "$(git ls-remote --tags origin "refs/tags/$TAG")" ] && TAG_EXISTS=1
if [ $((RELEASE_EXISTS + TAG_EXISTS)) -gt 0 ] && [ "$REPLACE" -eq 0 ]; then
  die "$TAG already exists on GitHub. Pass --replace to delete it and publish again."
fi

# Newest demo-* tag older than this one, for the change list.
PREV_TAG="" PREV_SHA=""
while read -r sha ref; do
  t="${ref#refs/tags/}"
  [[ "$t" == *"^{}" ]] && continue
  if [[ "$t" < "$TAG" ]]; then PREV_TAG="$t" PREV_SHA="$sha"; fi
done < <(git ls-remote --tags origin 'refs/tags/demo-*' | sort -k2)
# Annotated tags list the tag object; resolve it to the commit when we have it locally.
[ -n "$PREV_SHA" ] && PREV_SHA="$(git rev-parse --verify --quiet "$PREV_SHA^{commit}" || true)"

# --- Release notes ------------------------------------------------------------

NOTES="$(mktemp)"
trap 'rm -f "$NOTES"' EXIT
{
  echo "Demo build for the review of $DATE, from commit \`$SHORT\` ($SUBJECT)."
  [ "$DIRTY" -eq 1 ] && echo && echo "> **Built with uncommitted changes**: no commit matches this build exactly."
  echo
  echo "## Download"
  echo
  echo "| OS | File | Size |"
  echo "|---|---|---|"
  for z in "${ZIPS[@]}"; do
    n="$(basename "$z")"; os="${n#echoquest_demo_}"; os="${os%%_*}"
    case "$os" in linux) label=Linux ;; macos) label="macOS (Apple silicon)" ;; windows) label=Windows ;; esac
    echo "| $label | [$n](https://github.com/$GH_REPO/releases/download/$TAG/$n) | $(du -h "$z" | cut -f1 | tr -d ' ') |"
  done
  [ ${#MISSING[@]} -gt 0 ] && echo && echo "Not in this release: ${MISSING[*]}."
  echo
  echo "## How to run"
  echo
  echo "Extract the zip and follow \`README.txt\` inside it. Headphones recommended."
  echo
  echo "- **Windows**: if \"Windows protected your PC\" appears, click *More info*, then *Run anyway*."
  echo "- **macOS**: the first launch is blocked. Open *System Settings > Privacy & Security* and click *Open Anyway*."
  echo "- **Linux**: run \`./EchoQuest.x86_64\` from the extracted folder."
  echo
  echo "To report a problem, include \`BUILD_INFO.txt\` from the extracted folder."
  if [ -n "$PREV_SHA" ]; then
    changes="$(git log --no-merges --format='- %s (`%h`)' "$PREV_SHA..$SHA" | head -n 40)"
    if [ -n "$changes" ]; then
      echo
      echo "## Changes since $PREV_TAG"
      echo
      echo "$changes"
    fi
  fi
} > "$NOTES"

# --- Summary, confirmation, publish ------------------------------------------

echo "Repository: $GH_REPO"
echo "Release:    $TAG at $SHORT ($SUBJECT)$([ "$DIRTY" -eq 1 ] && echo ', pre-release')"
for z in "${ZIPS[@]}"; do echo "Asset:      ${z#$REPO/}"; done
[ ${#MISSING[@]} -gt 0 ] && echo "warning: no zip for ${MISSING[*]} on $DATE" >&2
[ $((RELEASE_EXISTS + TAG_EXISTS)) -gt 0 ] && echo "Replacing:  the existing $TAG release and tag"
echo
echo "----- release notes -----"
cat "$NOTES"
echo "-------------------------"

if [ "$DRY_RUN" -eq 1 ]; then echo "Dry run: nothing published."; exit 0; fi

if [ "$YES" -eq 0 ]; then
  [ -t 0 ] || die "not a terminal; pass --yes to publish without confirmation"
  read -r -p "Publish to $GH_REPO? [y/N] " answer
  [[ "$answer" =~ ^[Yy]$ ]] || { echo "Cancelled."; exit 1; }
fi

if [ "$RELEASE_EXISTS" -eq 1 ]; then
  gh release delete "$TAG" --repo "$GH_REPO" --cleanup-tag --yes
elif [ "$TAG_EXISTS" -eq 1 ]; then
  gh api -X DELETE "repos/$GH_REPO/git/refs/tags/$TAG" --silent
fi

FLAGS=(--latest)
[ "$DIRTY" -eq 1 ] && FLAGS=(--prerelease --latest=false)
gh release create "$TAG" "${ZIPS[@]}" --repo "$GH_REPO" --target "$SHA" \
  --title "EchoQuest demo $DATE" --notes-file "$NOTES" "${FLAGS[@]}"

echo
echo "Published: https://github.com/$GH_REPO/releases/tag/$TAG"
[ "$DIRTY" -eq 0 ] && echo "Always the newest demo: https://github.com/$GH_REPO/releases/latest"
