import array
import unittest

from stretch_pauses import find_pauses, stretch


class StretchPausesTests(unittest.TestCase):
    def test_extending_pause_does_not_repeat_a_breath(self):
        rate = 1000
        voice = array.array("h", [8000] * rate)
        breath = array.array("h", range(120, 200))
        gap = array.array("h", [0] * 40) + breath + array.array("h", [0] * 180)
        original = voice + gap + voice
        pauses = find_pauses(original, rate, -38, 0.28)
        self.assertEqual(pauses, [(1000, 1300)])

        result = stretch(original, rate, [(a, b, 0.9) for a, b in pauses])

        self.assertEqual(len(result), len(original) + 900)
        self.assertEqual(result.count(199), 1)
        self.assertEqual(result[:rate], voice)
        self.assertEqual(result[-rate:], voice)
        self.assertEqual(sum(sample != 0 for sample in result),
                         sum(sample != 0 for sample in original))
        self.assertEqual(original, voice + gap + voice)

    def test_multiple_pauses_preserve_the_words_between_them(self):
        words = [array.array("h", [level] * 500) for level in (6000, 9000, 12000)]
        gap = array.array("h", [0] * 300)
        original = words[0] + gap + words[1] + gap + words[2]
        pauses = find_pauses(original, 1000, -38, 0.28)

        result = stretch(original, 1000, [(a, b, 0.35) for a, b in pauses])

        self.assertEqual(len(result), len(original) + 700)
        for level in (6000, 9000, 12000):
            self.assertEqual(result.count(level), 500)

    def test_without_internal_pauses_audio_is_unchanged(self):
        original = array.array("h", [0] * 300 + [8000] * 500 + [0] * 300)
        pauses = find_pauses(original, 1000, -38, 0.28)
        self.assertEqual(pauses, [])
        self.assertEqual(stretch(original, 1000, []), original)


if __name__ == "__main__":
    unittest.main()
