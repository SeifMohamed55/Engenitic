CREATE OR REPLACE FUNCTION short_description(description TEXT)
RETURNS TEXT AS $$
SELECT string_agg(word, ' ') || '...'
FROM (
    SELECT unnest(string_to_array(description, ' ')) AS word
    LIMIT 3
) AS words;
$$ LANGUAGE SQL IMMUTABLE;
